using FileTreeMap;
using FileTreeMap.SubdivisionStrategies.SquarifiedSubdivision;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;



        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //CC.DirectoryPath = "E:\\TestEnum";
            //CC.DirectoryPath = "E:\\Path of Building";

            //CC.DirectoryPath = "E:\\GIMP 2";
            //CC.DirectoryPath = "D:\\";



            //var info = new DirectoryInfo("E:\\TestEnum");

            //var f = new FileTreeFactory();
            //var t = f.Create(info);



            //var tmf = new FileTreeMapFactory();
            //var map = tmf.CreateTreeMap(new Rect(0, 0, this.ActualWidth, this.ActualHeight), t, new SquarifiedSubdivisionStrategy());

            //var geomFac = new FileTreeMapGeometryFactory();
            //Visual? visual = null;

            //visual = await Task.Run(() =>
            //{
            //    return geomFac.CreateGeometry(map, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            //});



            //VH.Visual = visual;

            //VH.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;

            //var res = VisualTreeHelper.HitTest(VH, new Point(100, 100));
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //var pos = e.GetPosition(this);

            //var res = VisualTreeHelper.HitTest(VH, pos);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CC.DirectoryPath))
            {
                return;
            }

            CC.DirectoryPath = Directory.GetParent(CC.DirectoryPath)?.FullName;
        }
    }

    //public class FileTreeMapGeometryFactory
    //{
    //    public Visual CreateGeometry(ITreeMap<FileTreeItem> map, double pixelsPerDip)
    //    {

    //        var drawing = new DrawingVisual();

    //        var ctx = drawing.RenderOpen();

    //        var pal = new Brush[]
    //        {
    //            Brushes.Red,
    //            Brushes.Green,
    //            Brushes.Blue,
    //            Brushes.Gray,
    //            Brushes.MediumBlue,
    //            Brushes.Azure,
    //            Brushes.Cornsilk,
    //            Brushes.DarkSalmon,
    //            Brushes.Indigo,
    //            Brushes.MediumVioletRed
    //        };

    //        var ci = 0;

    //        foreach (var item in map)
    //        {
    //            var formattedText = new FormattedText(item.TitleDescription.Text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 16, Brushes.Black, pixelsPerDip);
    //            //var textGeometry = formattedText.BuildGeometry(item.TitleDescription.Position);



    //            //group.Children.Add(textGeometry);
    //            //var rectangleGeometry = new RectangleGeometry(item.RectangleDescription.Rectangle);

    //            //group.Children.Add(rectangleGeometry);

    //            var brush = pal[ci++];

    //            ctx.DrawRectangle(brush, new Pen(Brushes.Black, 0), item.RectangleDescription.Rectangle);
    //            ctx.DrawText(formattedText, item.TitleDescription.Position);
    //        }

    //        ctx.Close();

    //        return drawing;
    //    }
    //}

    public class VisualHost : UIElement
    {
        public Visual? Visual { get; set; }

        protected override int VisualChildrenCount
        {
            get { return Visual != null ? 1 : 0; }
        }

        protected override Visual? GetVisualChild(int index)
        {
            return Visual;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            
        }
    }

    public class FileTreeFactory
    {
        private readonly Queue<FileTreeItem> queue;

        public FileTreeFactory()
        {
            queue = new Queue<FileTreeItem>();
        }

        public FileTree Create(DirectoryInfo root)
        {
            var item = CreateItem(root);

            queue.Enqueue(item);

            while (queue.TryDequeue(out var queuedItem))
            {
                if (queuedItem.Info is DirectoryInfo directoryInfo)
                {
                    foreach (var subInfo in directoryInfo.EnumerateFileSystemInfos())
                    {
                        var subItem = CreateItem(subInfo);
                        queue.Enqueue(subItem);
                        queuedItem.Items.Add(subItem);

                        if (subInfo is FileInfo fileInfo)
                        {
                            queuedItem.Size += fileInfo.Length;
                        }
                    }
                }
            }

            return new FileTree
            {
                Root = item
            };
        }

        private FileTreeItem CreateItem(FileSystemInfo info)
        {
            return new FileTreeItem
            {
                Title = info.Name, // TODO : Read from info?
                Info = info,
                Size = info is FileInfo f ? f.Length : 0
            };
        }
    }



}
