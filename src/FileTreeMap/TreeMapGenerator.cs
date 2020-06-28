using FileTreeMap.SubdivisionStrategies;
using FileTreeMap.SubdivisionStrategies.SquarifiedSubdivision;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace FileTreeMap
{


    public interface ITreeItem<T> where T : ITreeItem<T>
    {
        string? Title { get; set; }

        double Size { get; set; }

        IList<T> Items { get; set; }
    }

    public interface ITree<T> where T : ITreeItem<T>
    {
        public T Root { get; set; }
    }

    public interface ITreeMapItem<T>
    {
        T TreeItem { get; }

        TreeMapTitleDescription TitleDescription { get; }

        TreeMapRectangleDescription RectangleDescription { get; }
    }

    public class FileTreeMapItem : ITreeMapItem<FileTreeItem>
    {
        public FileTreeItem TreeItem { get; private set; }

        public TreeMapTitleDescription TitleDescription { get; private set; }

        public TreeMapRectangleDescription RectangleDescription { get; private set; }

        public FileTreeMapItem(FileTreeItem treeItem, TreeMapTitleDescription titleDescription, TreeMapRectangleDescription rectangleDescription)
        {
            TreeItem = treeItem;
            TitleDescription = titleDescription;
            RectangleDescription = rectangleDescription;
        }
    }


    public class TreeMapTitleDescription
    {
        public string? Text { get; set; }

        public System.Windows.Point Position { get; set; }

        public double Size { get; set; }
    }

    public class TreeMapRectangleDescription
    {
        public Rect Rectangle { get; set; }

        public Brush? Brush { get; set; }
    }

    public interface ITreeMap<T> : IEnumerable<ITreeMapItem<T>> where T : ITreeItem<T>
    {
        ITreeMapItem<T>? HitTest(System.Windows.Point point, ITree<T> tree);
    }

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

            //var queue = new Queue<FileTreeItem>();
            //queue.Enqueue(tree.Root);
            FileTreeMapItem? hit = null;
            FileTreeMapItem? previousHit = null;

            //while (queue.TryDequeue(out var treeItem))
            //{

            //}

            var testList = new List<FileTreeItem>();
            testList.Add(tree.Root);

            while (testList.Count > 0)
            {
                hit = null;

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

    public interface ITreeMapFactory<T> where T : ITreeItem<T>
    {
        ITreeMap<T> CreateTreeMap(Rect rectangle, ITree<T> tree, ISubdivisionStrategy subdivisionStrategy, CancellationToken cancellationToken = default);
    }

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
        }

        public ITreeMap<FileTreeItem> CreateTreeMap(Rect rectangle, ITree<FileTreeItem> tree, ISubdivisionStrategy subdivisionStrategy, CancellationToken cancellationToken = default)
        {
            if (tree.Root == null)
            {
                return new FileTreeMap();
            }

            fileTree = (FileTree)tree;
            datesSpan = fileTree.NewestItemTimestamp - fileTree.OldestItemTimestamp;

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
            var grade = (double)((fileTree!.NewestItemTimestamp - treeItem.Info!.LastWriteTime).TotalDays) / (datesSpan.TotalDays);
            var index = (int)(palette.Length * grade);
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



    public class FileTreeItem : ITreeItem<FileTreeItem>
    {
        public string? Title { get; set; }

        public double Size { get; set; }

        public FileSystemInfo? Info { get; set; }

        public FileTreeItem? Parent { get; set; }

        public IList<FileTreeItem> Items { get; set; } = new List<FileTreeItem>();
    }

    //public class FileTreeMapFactory : TreeMapFactory<FileTreeItem>
    //{

    //}

    public class FileTree : ITree<FileTreeItem>
    {
        public FileTreeItem? Root { get; set; }

        public DateTime OldestItemTimestamp { get; set; }

        public DateTime NewestItemTimestamp { get; set; }
    }

    public class Test
    {
        public Test()
        {
            //var s = new SquarifiedSubdivisionStrategy();
            //var t = new FileTree();
            //var f = new FileTreeMapFactory();
            //var m = f.CreateTreeMap(t, s);
            
        }
    }
}
