using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Printing;
using System.Threading;

namespace FileTreeMap
{
    public class FileTreeFactory
    {
        private readonly Queue<FileTreeItem> queue;

        public FileTreeFactory()
        {
            queue = new Queue<FileTreeItem>();
        }

        public FileTree CreateFileTree(DirectoryInfo root, CancellationToken cancellationToken = default)
        {
            if (!root.Exists)
            {
                return new FileTree();
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
                    Debug.WriteLine("CANCELLATION REQEUSTED1");
                    break;
                }

                if (queuedItem.Info is DirectoryInfo directoryInfo)
                {
                    foreach (var subInfo in directoryInfo.EnumerateFileSystemInfos("*", options))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            Debug.WriteLine("CANCELLATION REQEUSTED2");
                            break;
                        }

                        var subItem = CreateItem(queuedItem, subInfo);
                        queue.Enqueue(subItem);
                        queuedItem.Items.Add(subItem);

                        oldest = oldest > subInfo.LastWriteTime ? subInfo.LastWriteTime : oldest;
                        newest = newest < subInfo.LastWriteTime ? subInfo.LastWriteTime : newest;

                        if (subInfo is FileInfo fileInfo)
                        {
                            //queuedItem.Size += fileInfo.Length;
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
            }

            return new FileTree
            {
                Root = item,
                OldestItemTimestamp = oldest,
                NewestItemTimestamp = newest
            };
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
