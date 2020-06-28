using System.Collections.Generic;
using System.IO;

namespace DemoControls.Trees
{
    public class FileTreeItem : ITreeItem<FileTreeItem>
    {
        public string? Title { get; set; }

        public double Size { get; set; }

        public FileSystemInfo? Info { get; set; }

        public FileTreeItem? Parent { get; set; }

        public IList<FileTreeItem> Items { get; set; } = new List<FileTreeItem>();
    }
}
