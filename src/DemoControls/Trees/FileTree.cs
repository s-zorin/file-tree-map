using System;

namespace DemoControls.Trees
{
    public class FileTree : ITree<FileTreeItem>
    {
        public FileTreeItem? Root { get; set; }

        public DateTime OldestItemTimestamp { get; set; }

        public DateTime NewestItemTimestamp { get; set; }
    }
}
