using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DemoControls.Trees
{
    public class FileTreeFactory
    {
        public FileTreeFactory()
        {
            
        }

        public FileTree CreateFileTree(DirectoryInfo root, CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"Started creation of file tree. ({root.FullName})");

            var queue = new Queue<FileTreeItem>();

            if (!root.Exists)
            {
                return FileTree.Empty;
            }

            var item = CreateItem(null, root);
            queue.Enqueue(item);

            var oldest = DateTime.MaxValue;
            var newest = DateTime.MinValue;

            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true
            };

            while (queue.TryDequeue(out var queuedItem))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine($"CANCELLATION REQEUSTED1 ({root.FullName})");
                    break;
                }

                if (queuedItem.Info is DirectoryInfo directoryInfo)
                {
                    oldest = oldest > queuedItem.Info.LastWriteTime ? queuedItem.Info.LastWriteTime : oldest;
                    newest = newest < queuedItem.Info.LastWriteTime ? queuedItem.Info.LastWriteTime : newest;

                    try
                    {
                        foreach (var subInfo in directoryInfo.EnumerateFileSystemInfos("*", options))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                Debug.WriteLine($"CANCELLATION REQEUSTED2 ({root.FullName})");
                                break;
                            }

                            var subItem = CreateItem(queuedItem, subInfo);
                            queue.Enqueue(subItem);
                            queuedItem.Items.Add(subItem);

                            if (subInfo is FileInfo fileInfo)
                            {
                                subItem.Size = fileInfo.Length;

                                var parent = subItem.Parent;
                                while (parent != null)
                                {
                                    parent.Size += fileInfo.Length;
                                    parent = parent.Parent;
                                }

                            }
                        }
                    }
                    catch (IOException)
                    {
                        // ignored
                    }

                }
            }

            Debug.WriteLine("Exited loop.");

            return new FileTree(item, oldest, newest);
        }

        private FileTreeItem CreateItem(FileTreeItem? parent, FileSystemInfo info)
        {
            return new FileTreeItem
            {
                Parent = parent,
                Title = info.Name, // TODO : Read from info?
                Info = info,
                Size = info is FileInfo f ? f.Length : 0
            };
        }
    }
}
