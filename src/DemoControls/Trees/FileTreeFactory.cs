using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DemoControls.Trees
{
    public class FileTreeFactory
    {
        public FileTree CreateFileTree(DirectoryInfo root, CancellationToken cancellationToken = default)
        {
            var queue = new Queue<FileTreeItem>();

            if (!root.Exists)
            {
                return FileTree.Empty;
            }

            var rootItem = new FileTreeItem(null, root);
            queue.Enqueue(rootItem);

            var oldestFileTimestamp = DateTime.MaxValue;
            var newestFileTimestamp = DateTime.MinValue;

            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true
            };

            while (queue.TryDequeue(out var parentItem))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                oldestFileTimestamp = oldestFileTimestamp > parentItem.Info.LastWriteTime ? parentItem.Info.LastWriteTime : oldestFileTimestamp;
                newestFileTimestamp = newestFileTimestamp < parentItem.Info.LastWriteTime ? parentItem.Info.LastWriteTime : newestFileTimestamp;

                if (parentItem.Info is DirectoryInfo directoryInfo)
                {
                    try
                    {
                        var items = ProcessFileInfos(parentItem, directoryInfo.EnumerateFileSystemInfos("*", options));
                        
                        foreach (var item in items)
                        {
                            queue.Enqueue(item);
                        }
                    }
                    catch (IOException)
                    {
                        // ignored
                    }

                }
            }

            return new FileTree(rootItem, oldestFileTimestamp, newestFileTimestamp);
        }

        private IEnumerable<FileTreeItem> ProcessFileInfos(FileTreeItem parent, IEnumerable<FileSystemInfo> infos)
        {
            foreach (var subInfo in infos)
            {
                var subItem = new FileTreeItem(parent, subInfo);
                parent.Items.Add(subItem);
                yield return subItem;
            }
        }
    }
}
