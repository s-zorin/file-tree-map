using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DemoControls.Trees
{
    public class FileTreeItem : ITreeItem<FileTreeItem>
    {
        private readonly ObservableCollection<FileTreeItem> collection = new ObservableCollection<FileTreeItem>();

        public string Title { get; }

        public double Size { get; private set; }

        public FileSystemInfo Info { get; }

        public FileTreeItem? Parent { get; }

        public IList<FileTreeItem> Items => collection;

        public FileTreeItem(FileTreeItem? parent, FileSystemInfo info)
        {
            Parent = parent;
            Title = info.Name;
            Size = info is FileInfo file ? file.Length : 0;
            Info = info;

            collection.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            var parent = this;
            var addedItem = args.NewItems?.OfType<FileTreeItem>().SingleOrDefault();
            var removedItem = args.OldItems?.OfType<FileTreeItem>().SingleOrDefault();
            var size = addedItem?.Size ?? 0 - removedItem?.Size ?? 0;

            while (parent != null)
            {
                parent.Size += size;
                parent = parent.Parent;
            }
        }
    }
}
