using DemoControls.SubdivisionStrategies;
using DemoControls.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace DemoControls.TreeMaps
{
    public class FileTreeMapFactory : ITreeMapFactory<FileTreeItem>
    {
        private TimeSpan directoryAge;
        private DateTime newestFileTimestamp;

        private readonly FileTreeMapPalette palette;

        private class FileTreeItemRectangle
        {
            public FileTreeItem TreeItem { get; set; }

            public Rect AssociatedRectangle { get; set; }

            public Rect RectangleToSubdivide => GetRectangleToSubdivide();

            private Rect GetRectangleToSubdivide()
            {
                // Leave space for title

                var x = AssociatedRectangle.X;
                var y = AssociatedRectangle.Y + 16;
                var width = AssociatedRectangle.Width;
                var height = Math.Max(AssociatedRectangle.Height - 16, 0);

                return new Rect(x, y, width, height);
            }

            public FileTreeItemRectangle(FileTreeItem treeItem, Rect associatedRectangle)
            {
                TreeItem = treeItem;
                AssociatedRectangle = associatedRectangle;
            }
        }

        private class FileTreeItemArea
        {
            public FileTreeItem? Item { get; set; }

            public double Area { get; set; }
        }

        public FileTreeMapFactory()
        {
            palette = new FileTreeMapPalette(50);
        }

        public ITreeMap<FileTreeItem> CreateTreeMap(Rect rectangle, ITree<FileTreeItem> tree, ISubdivisionStrategy subdivisionStrategy, CancellationToken cancellationToken = default)
        {
            if (tree.Root == null)
            {
                return new FileTreeMap();
            }

            var fileTree = (FileTree)tree;
            directoryAge = fileTree.NewestFileTimestamp - fileTree.OldestFileTimestamp;
            newestFileTimestamp = fileTree.NewestFileTimestamp;

            var map = new FileTreeMap();
            var queue = new Queue<FileTreeItemRectangle>();
            queue.Enqueue(new FileTreeItemRectangle(tree.Root, rectangle));

            while (queue.TryDequeue(out var itemRectangle))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var mapItem = CreateMapItem(itemRectangle);
                map.Add(mapItem);

                var pairs = Subdivide(itemRectangle, subdivisionStrategy);

                foreach (var subPair in pairs)
                {
                    queue.Enqueue(subPair);
                }
            }

            return map;
        }

        private FileTreeMapItem CreateMapItem(FileTreeItemRectangle pair)
        {
            var title = new TreeMapTitleDescription
            {
                Position = pair.AssociatedRectangle.Location,
                Size = 16,
                Text = pair.TreeItem.Title
            };

            var rectangle = new TreeMapRectangleDescription
            {
                Rectangle = pair.AssociatedRectangle,
                Brush = ChooseBrush(pair.TreeItem)
            };

            return new FileTreeMapItem(pair.TreeItem, title, rectangle);
        }

        private Brush ChooseBrush(FileTreeItem treeItem)
        {
            if (directoryAge.TotalDays < 1)
            {
                return palette[0];
            }

            var age = newestFileTimestamp - treeItem.Info.LastWriteTime;
            var grade = age.TotalDays / directoryAge.TotalDays;
            var index = (int)((palette.Length - 1) * grade);
            return palette[index];
        }

        private IEnumerable<FileTreeItemRectangle> Subdivide(FileTreeItemRectangle itemRectangle, ISubdivisionStrategy strategy)
        {
            const int MIN_AREA = 64 * 64;

            if (itemRectangle.RectangleToSubdivide.Width < 4 ||
                itemRectangle.RectangleToSubdivide.Height < 4)
            {
                return Enumerable.Empty<FileTreeItemRectangle>();
            }

            var rectangleArea = itemRectangle.RectangleToSubdivide.Width * itemRectangle.RectangleToSubdivide.Height;

            var totalSize = itemRectangle.TreeItem.Items.Sum(i => i.Size);
            var conversionRatio = rectangleArea / totalSize;

            var itemsWithSize = itemRectangle.TreeItem.Items.Where(i => i.Size > 0).OrderByDescending(i => i.Size).ToList();

            if (!itemsWithSize.Any())
            {
                return Enumerable.Empty<FileTreeItemRectangle>();
            }

            var subrectangleAreas = itemsWithSize.Select(i => new FileTreeItemArea { Item = i, Area = i.Size * conversionRatio }).ToList();

            // Удаляем самые маленькие прямоугольники и перераспределяем их площадь по остальным прямоугольникам.
            while (subrectangleAreas.Min(sa => sa.Area) < MIN_AREA && subrectangleAreas.Count > 1)
            {
                var last = subrectangleAreas.Last();
                subrectangleAreas.Remove(last);
                var areaToRedistribute = last.Area / subrectangleAreas.Count;
                foreach (var sub in subrectangleAreas)
                {
                    sub.Area += areaToRedistribute;
                }
            }

            var rectangles = strategy.Subdivide(itemRectangle.RectangleToSubdivide, subrectangleAreas.Select(a => a.Area));
            rectangles = DownsizeRectangles(rectangles);
            var pairs = subrectangleAreas.Zip(rectangles, (item, rectangle) => new FileTreeItemRectangle(item.Item!, rectangle));
            return pairs;
        }

        private IEnumerable<Rect> DownsizeRectangles(IEnumerable<Rect> rectangles)
        {
            foreach (var r in rectangles)
            {
                yield return new Rect(r.X + 2, r.Y + 2, Math.Max(r.Width - 4, 0), Math.Max(r.Height - 4, 0));
            }
        }
    }
}
