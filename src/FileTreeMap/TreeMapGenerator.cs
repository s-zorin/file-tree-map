using FileTreeMap.SubdivisionStrategies;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace FileTreeMap
{


    public interface ITreeItem
    {
        string Title { get; }

        double Size { get; }

        IEnumerable<ITreeItem> Items { get; }
    }

    public interface ITree<T> where T : ITreeItem
    {

    }

    public interface ITreeMapItem<T>
    {
        T TreeItem { get; }

        TreeMapTitleDescription TitleDescription { get; }

        TreeMapRectangleDescription RectangleDescription { get; }
    }


    public class TreeMapTitleDescription
    {
        string? Text { get; }

        Point Position { get; }

        double Size { get; }
    }

    public class TreeMapRectangleDescription
    {
        Point Position { get; }

        double Width { get; }

        double Height { get; }
    }

    public interface ITreeMap<T> : IEnumerable<ITreeMapItem<T>> where T : ITreeItem
    {
    }

    public interface ITreeMapFactory<T> where T : ITreeItem
    {
        ITreeMap<T> CreateTreeMap(ITree<T> tree, ISubdivisionStrategy subdivisionStrategy);
    }

    public class TreeMapFactory<T> : ITreeMapFactory<T> where T : ITreeItem
    {
        public ITreeMap<T> CreateTreeMap(ITree<T> tree, ISubdivisionStrategy subdivisionStrategy)
        {
            throw new NotImplementedException();
        }
    }



    public class FileTreeItem : ITreeItem
    {
        public string Title => throw new NotImplementedException();

        public double Size => throw new NotImplementedException();

        public IEnumerable<ITreeItem> Items => throw new NotImplementedException();
    }

    public class FileTreeMapFactory : TreeMapFactory<FileTreeItem>
    {

    }

    public class FileTree : ITree<FileTreeItem>
    {

    }

    public delegate IEnumerable<Rect> RectangleRowLayout(IEnumerable<double> rectangleAreas);

    public class Test
    {
        public Test()
        {
            var s = new SquarifiedSubdivisionStrategy();
            var t = new FileTree();
            var f = new FileTreeMapFactory();
            var m = f.CreateTreeMap(t, s);
            
        }
    }
}
