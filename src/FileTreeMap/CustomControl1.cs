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

namespace FileTreeMap
{
    public class CustomControl1 : Control
    {
        private readonly FileTreeFactory fileTreeFactory;
        private readonly FileTreeMapFactory fileTreeMapFactory;
        private readonly SquarifiedSubdivisionStrategy subdivisionStrategy;
        private CancellationTokenSource visualUpdateCancellationTokenSource;
        private CancellationTokenSource fullUpdateCancellationTokenSource;

        FileTree? fileTree;
        FileTreeMap? fileTreeMap;
        VisualBrush? visualBrush;
        Rectangle? visualHost;
        UIElement? busyIndicator;
        FileSystemWatcher? watcher;

        int busyCount = 0;

        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }

        public CustomControl1()
        {
            fileTreeFactory = new FileTreeFactory();
            fileTreeMapFactory = new FileTreeMapFactory();
            subdivisionStrategy = new SquarifiedSubdivisionStrategy();
            visualUpdateCancellationTokenSource = new CancellationTokenSource();
            fullUpdateCancellationTokenSource = new CancellationTokenSource();

            SizeChanged += OnSizeChanged;
            MouseDoubleClick += OnDoubleClick;
            Unloaded += OnUnloaded;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            watcher = new FileSystemWatcher();
            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Deleted += OnFileChanged;
            watcher.Renamed += OnFileChanged;
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // TODO : Можно пропатчить существующее дерево новыми изменениями и перерисовать экран/
            // Жаль, что у меня нет на это времени, поэтому будет полный апдейт.
            await Dispatcher.InvokeAsync(async () =>
            {
                await FullUpdateAsync();
            });
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            watcher?.Dispose();
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs args)
        {
            if (fileTreeMap == null)
            {
                return;
            }

            if (fileTree == null)
            {
                return;
            }

            var hitResult = fileTreeMap.HitTest(args.GetPosition(this), fileTree);

            if (hitResult?.TreeItem?.Info != null)
            {
                DirectoryPath = hitResult.TreeItem.Info.FullName;
            }
        }

        private async void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            await VisualUpdateAsync();
        }

        public string? DirectoryPath
        {
            get { return (string?)GetValue(DirectoryPathProperty); }
            set { SetValue(DirectoryPathProperty, value); }
        }

        public static readonly DependencyProperty DirectoryPathProperty =
            DependencyProperty.Register("DirectoryPath", typeof(string), typeof(CustomControl1), new PropertyMetadata(null, OnDirectoryPathChanged));

        private static async void OnDirectoryPathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is CustomControl1 control)
            {
                await control.FullUpdateAsync();
                control.SetupFileSystemWatcher();
            }
        }

        private void SetupFileSystemWatcher()
        {
            if (watcher == null)
            {
                return;
            }

            if (!Directory.Exists(DirectoryPath))
            {
                return;
            }

            watcher.Path = DirectoryPath;
            watcher.NotifyFilter = NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
            watcher.EnableRaisingEvents = true;
        }

        public override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            visualBrush = GetTemplateChild("VisualBrush") as VisualBrush;
            visualHost = GetTemplateChild("VisualHost") as Rectangle;
            busyIndicator = GetTemplateChild("BusyIndicator") as UIElement;

            await FullUpdateAsync();
        }

        private async Task VisualUpdateAsync()
        {
            visualUpdateCancellationTokenSource.Cancel();
            visualUpdateCancellationTokenSource.Dispose();
            visualUpdateCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = visualUpdateCancellationTokenSource.Token;

            try
            {
                ShowBusyIndicator();
                await VisualUpdateAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            finally
            {
                HideBusyIndicator();
            }
        }

        private void ShowBusyIndicator()
        {
            busyCount++;
            
            if (busyIndicator == null)
            {
                return;
            }

            busyIndicator.Visibility = Visibility.Visible;
        }

        private void HideBusyIndicator()
        {
            busyCount--;

            if (busyIndicator == null)
            {
                return;
            }

            if (busyCount == 0)
            {
                busyIndicator.Visibility = Visibility.Hidden;
            }
        }

        private async Task VisualUpdateAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (visualHost == null)
            {
                return;
            }

            if (visualBrush == null)
            {
                return;
            }

            if (fileTree == null)
            {
                return;
            }

            var drawingVisual = new DrawingVisual();
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var hostRectangle = new Rect(0, 0, visualHost.ActualWidth, visualHost.ActualHeight);

            fileTreeMap = await Task.Run(() =>
            {
                return (FileTreeMap)fileTreeMapFactory.CreateTreeMap(
                    hostRectangle,
                    fileTree,
                    subdivisionStrategy,
                    cancellationToken);
            }, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                DrawFileTreeMap(drawingContext, fileTreeMap, pixelsPerDip);
            }

            visualBrush.Visual = drawingVisual;
        }

        private void DrawFileTreeMap(DrawingContext drawingContext, FileTreeMap fileTreeMap, double pixelsPerDip)
        {
            foreach (var item in fileTreeMap)
            {
                var brush = item.RectangleDescription.Brush;
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 1), item.RectangleDescription.Rectangle);

                if (item.RectangleDescription.Rectangle.Height > 16 && item.RectangleDescription.Rectangle.Width > 16)
                {
                    var formattedText = new FormattedText(
                        item.TitleDescription.Text,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        12,
                        Brushes.Black,
                        pixelsPerDip);

                    formattedText.MaxTextWidth = item.RectangleDescription.Rectangle.Width;
                    formattedText.MaxLineCount = 1;
                    drawingContext.DrawText(formattedText, item.TitleDescription.Position);
                }                
            }
        }

        private async Task FullUpdateAsync()
        {
            fullUpdateCancellationTokenSource.Cancel();
            fullUpdateCancellationTokenSource.Dispose();
            fullUpdateCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = fullUpdateCancellationTokenSource.Token;

            try
            {
                ShowBusyIndicator();
                await FullUpdateAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            finally
            {
                HideBusyIndicator();
            }
        }

        private async Task FullUpdateAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!Directory.Exists(DirectoryPath))
            {
                return;
            }

            if (visualHost == null)
            {
                return;
            }

            var rootDirectoryInfo = new DirectoryInfo(DirectoryPath);
            fileTree = await Task.Run(() => fileTreeFactory.CreateFileTree(rootDirectoryInfo, cancellationToken), cancellationToken);

            await VisualUpdateAsync(cancellationToken);
        }
    }
}
