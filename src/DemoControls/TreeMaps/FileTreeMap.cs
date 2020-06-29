using DemoControls.Trees;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

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

        public ITreeMapItem<FileTreeItem>? HitTest(Point point, ITree<FileTreeItem> tree)
        {
            if (tree.Root == null)
            {
                return null;
            }

            FileTreeItem? previousHit = null;
            var treeItemsToTest = new List<FileTreeItem>
            {
                tree.Root
            };

            while (treeItemsToTest.Count > 0)
            {
                var hit = HitTestTreeItems(treeItemsToTest, point);
                if (hit == null)
                {
                    break;
                }

                previousHit = hit;
                treeItemsToTest.Clear();
                treeItemsToTest.AddRange(hit.Items);
            }

            if (previousHit == null)
            {
                return null;
            }

            if (dictionary.TryGetValue(previousHit, out var result))
            {
                return result;
            }

            return null;
        }

        public IEnumerator<ITreeMapItem<FileTreeItem>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        private bool HitTestTreeItem(FileTreeItem treeItem, Point point)
        {
            if (dictionary.TryGetValue(treeItem, out var mapItem))
            {
                return mapItem.RectangleDescription.Rectangle.Contains(point);
            }

            return false;
        }

        private FileTreeItem? HitTestTreeItems(IEnumerable<FileTreeItem> treeItems, Point point)
        {
            foreach (var treeItem in treeItems)
            {
                if (HitTestTreeItem(treeItem, point))
                {
                    return treeItem;
                }
            }

            return null;
        }
    }
}
