using System;

namespace DemoControls.Trees
{
    public class FileTree : ITree<FileTreeItem>
    {
        public FileTreeItem? Root { get; }

        public DateTime OldestFileTimestamp { get; }

        public DateTime NewestFileTimestamp { get; }

        public FileTree(FileTreeItem? root, DateTime oldestFileTimestamp, DateTime newestFileTimestamp)
        {
            Root = root;
            OldestFileTimestamp = oldestFileTimestamp;
            NewestFileTimestamp = newestFileTimestamp;
        }

        public static FileTree Empty => new FileTree(null, DateTime.MinValue, DateTime.MaxValue);
    }
}
