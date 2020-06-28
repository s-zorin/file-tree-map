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
        private FileTree? fileTree;
        private TimeSpan datesSpan;

        private readonly Brush[] palette;

        private struct Pair
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

            public Pair(FileTreeItem treeItem, Rect associatedRectangle)
            {
                TreeItem = treeItem;
                AssociatedRectangle = associatedRectangle;
            }
        }

        public FileTreeMapFactory()
        {
            palette = new Brush[50];

            for (int i = 0; i < palette.Length; i++)
            {
                var g  =  (byte)(60 + 180 * (i / (double)palette.Length));
                palette[i] = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, g, 0));
            }

            palette = palette.Reverse().ToArray();
        }

        public ITreeMap<FileTreeItem> CreateTreeMap(Rect rectangle, ITree<FileTreeItem> tree, ISubdivisionStrategy subdivisionStrategy, CancellationToken cancellationToken = default)
        {
            if (tree.Root == null)
            {
                return new FileTreeMap();
            }

            fileTree = (FileTree)tree;
            datesSpan = fileTree.NewestFileTimestamp - fileTree.OldestFileTimestamp;

            var map = new FileTreeMap();
            var queue = new Queue<Pair>();
            queue.Enqueue(new Pair(tree.Root, rectangle));

            while (queue.TryDequeue(out var pair))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var mapItem = CreateMapItem(pair);
                map.Add(mapItem);

                var pairs = Subdivide(pair, subdivisionStrategy);

                foreach (var subPair in pairs)
                {
                    queue.Enqueue(subPair);
                }
            }

            return map;
        }

        private FileTreeMapItem CreateMapItem(Pair pair)
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
            if (datesSpan.TotalDays < 1)
            {
                return palette[0];
            }

            var grade = (double)((fileTree!.NewestFileTimestamp - treeItem.Info!.LastWriteTime).TotalDays) / (datesSpan.TotalDays);
            var index = (int)((palette.Length - 1) * grade);
            return palette[index];
        }

        private class FileTreeItemArea
        {
            public FileTreeItem? Item { get; set; }

            public double Area { get; set; }
        }

        private IEnumerable<Pair> Subdivide(Pair pair, ISubdivisionStrategy strategy)
        {
            // Нужно удалить Н-ное количество подквадратов с минимальной площадью, так чтобы все оставшиеся квадраты были площадью как минимум 32 на 32.

            const int MIN_AREA = 64 * 64;

            if (pair.RectangleToSubdivide.Width < 4 ||
                pair.RectangleToSubdivide.Height < 4)
            {
                return Enumerable.Empty<Pair>();
            }

            var rectangleArea = pair.RectangleToSubdivide.Width * pair.RectangleToSubdivide.Height;

            var totalSize = pair.TreeItem.Items.Sum(i => i.Size);
            var conversionRatio = rectangleArea / totalSize;

            var itemsWithSize = pair.TreeItem.Items.Where(i => i.Size > 0).OrderByDescending(i => i.Size).ToList();



            if (!itemsWithSize.Any())
            {
                return Enumerable.Empty<Pair>();
            }




            // Cleanup small rects
            var subrectangleAreas = itemsWithSize.Select(i => new FileTreeItemArea { Item = i, Area = i.Size * conversionRatio }).ToList();

            while (subrectangleAreas.Min(sa => sa.Area) < MIN_AREA && subrectangleAreas.Count > 1)
            {
                var last = subrectangleAreas[subrectangleAreas.Count - 1];
                subrectangleAreas.Remove(last);
                var areaToRedistribute = last.Area / subrectangleAreas.Count;
                foreach (var sub in subrectangleAreas)
                {
                    sub.Area += areaToRedistribute;
                }
            }




            var rectangles = strategy.Subdivide(pair.RectangleToSubdivide, subrectangleAreas.Select(a => a.Area));
            rectangles = DownsizeRectangles(rectangles);
            var pairs = subrectangleAreas.Zip(rectangles, (item, rectangle) => new Pair(item.Item, rectangle));
            return pairs/*.Where(p => p.AssociatedRectangle.Width >= 16 && p.AssociatedRectangle.Height >= 16)*/;
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
