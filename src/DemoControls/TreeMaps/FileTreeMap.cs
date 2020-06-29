using DemoControls.Trees;
using System.Collections;
using System.Collections.Generic;

namespace DemoControls.TreeMaps
{
    public class FileTreeMap : ITreeMap<FileTreeItem>
    {
        private readonly Dictionary<FileTreeItem, FileTreeMapItem> dictionary = new Dictionary<FileTreeItem, FileTreeMapItem>();
        private readonly List<FileTreeMapItem> items = new List<FileTreeMapItem>();

        public void Add(FileTreeMapItem item)
        {
            items.Add(item);
            dictionary.Add(item.TreeItem, item);
        }

        public ITreeMapItem<FileTreeItem>? HitTest(System.Windows.Point point, ITree<FileTreeItem> tree)
        {
            if (tree.Root == null)
            {
                return null;
            }

            FileTreeMapItem? previousHit = null;


            var testList = new List<FileTreeItem>
            {
                tree.Root
            };

            while (testList.Count > 0)
            {
                FileTreeMapItem? hit = null;

                foreach (var treeItem in testList)
                {
                    if (dictionary.TryGetValue(treeItem, out var mapItem))
                    {
                        if (mapItem.RectangleDescription.Rectangle.Contains(point))
                        {
                            hit = mapItem;
                            break;
                        }
                    }
                }

                if (hit == null)
                {
                    break;
                }

                previousHit = hit;

                testList.Clear();

                if (hit != null)
                {
                    testList.AddRange(hit.TreeItem.Items);
                }
            }

            return previousHit;
        }

        public IEnumerator<ITreeMapItem<FileTreeItem>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
