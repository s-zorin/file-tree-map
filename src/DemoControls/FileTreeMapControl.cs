using DemoControls.SubdivisionStrategies.SquarifiedSubdivision;
using DemoControls.TreeMaps;
using DemoControls.Trees;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DemoControls
{
    public class FileTreeMapControl : Control
    {
        private const string VISUAL_HOST_PART = "VisualHost";
        private const string BUSY_INDICATOR_PART = "BusyIndicator";

        private readonly Debouncer sizeDebouncer;
        private readonly Debouncer fullDebouncer;
        private readonly FileTreeFactory fileTreeFactory;
        private readonly FileTreeMapFactory fileTreeMapFactory;
        private readonly SquarifiedSubdivisionStrategy subdivisionStrategy;
        private CancellationTokenSource visualUpdateCancellationTokenSource;
        private CancellationTokenSource fullUpdateCancellationTokenSource;

        private FileTree? fileTree;
        private FileTreeMap? fileTreeMap;
        private Rectangle? visualHost;
        private UIElement? busyIndicator;
        private FileSystemWatcher? watcher;

        int busyCount = 0;

        public static readonly DependencyProperty DirectoryPathProperty = DependencyProperty.Register(
            nameof(DirectoryPath),
            typeof(string),
            typeof(FileTreeMapControl),
            new PropertyMetadata(null, OnDirectoryPathChanged, CoerceDirectoryPath));

        public string? DirectoryPath
        {
            get => (string?)GetValue(DirectoryPathProperty);
            set => SetValue(DirectoryPathProperty, value);
        }

        static FileTreeMapControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileTreeMapControl), new FrameworkPropertyMetadata(typeof(FileTreeMapControl)));
        }

        public FileTreeMapControl()
        {
            sizeDebouncer = new Debouncer(TimeSpan.FromSeconds(0.5));
            fullDebouncer = new Debouncer(TimeSpan.FromSeconds(1));
            fileTreeFactory = new FileTreeFactory();
            fileTreeMapFactory = new FileTreeMapFactory();
            subdivisionStrategy = new SquarifiedSubdivisionStrategy();
            visualUpdateCancellationTokenSource = new CancellationTokenSource();
            fullUpdateCancellationTokenSource = new CancellationTokenSource();

            SizeChanged += OnSizeChanged;
            MouseDoubleClick += OnDoubleClick;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            watcher = new FileSystemWatcher();
            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Deleted += OnFileChanged;
            watcher.Renamed += OnFileChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            watcher?.Dispose();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            fullDebouncer.Enqueue(async () => await Dispatcher.InvokeAsync(FullUpdateAsync));
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

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            sizeDebouncer.Enqueue(async () => await Dispatcher.InvokeAsync(VisualUpdateAsync));
        }

        private static async void OnDirectoryPathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is FileTreeMapControl control)
            {
                await control.FullUpdateAsync();
                control.SetupFileSystemWatcher();
            }
        }

        private static object CoerceDirectoryPath(DependencyObject dependencyObject, object baseValue)
        {
            if (baseValue is string s)
            {
                return s.Trim();
            }

            return baseValue;
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

            watcher.IncludeSubdirectories = true;
            watcher.Path = DirectoryPath;
            watcher.NotifyFilter = NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
            watcher.EnableRaisingEvents = true;
        }

        public override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            visualHost = GetTemplateChild(VISUAL_HOST_PART) as Rectangle;
            busyIndicator = GetTemplateChild(BUSY_INDICATOR_PART) as UIElement;

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

        private async Task<FileTreeMap?> CreateFileTreeMapAsync(CancellationToken cancellationToken)
        {
            if (visualHost == null)
            {
                return null;
            }

            if (fileTree == null)
            {
                return null;
            }

            var hostRectangle = new Rect(0, 0, visualHost.ActualWidth, visualHost.ActualHeight);

            return await Task.Run(() =>
            {
                return (FileTreeMap)fileTreeMapFactory.CreateTreeMap(
                    hostRectangle,
                    fileTree,
                    subdivisionStrategy,
                    cancellationToken);
            }, cancellationToken);
        }

        private async Task<ImageBrush?> CreateImageBrush(FileTreeMap fileTreeMap, CancellationToken cancellationToken)
        {
            if (visualHost == null)
            {
                return null;
            }

            if (fileTreeMap == null)
            {
                return null;
            }

            var bitmapWidth = (int)visualHost.ActualWidth;
            var bitmapHeight = (int)visualHost.ActualHeight;
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            return await Task.Run(() =>
            {
                var drawingVisual = new DrawingVisual();
                using (var drawingContext = drawingVisual.RenderOpen())
                {
                    DrawFileTreeMap(drawingContext, fileTreeMap, pixelsPerDip);
                }

                var bitmap = new RenderTargetBitmap(bitmapWidth, bitmapHeight, 96 * pixelsPerDip, 96 * pixelsPerDip, PixelFormats.Pbgra32);
                bitmap.Render(drawingVisual);
                bitmap.Freeze();

                var brush = new ImageBrush(bitmap);
                brush.Freeze();
                return brush;
            }, cancellationToken);
        }

        private async Task VisualUpdateAsync(CancellationToken cancellationToken)
        {
            fileTreeMap = await CreateFileTreeMapAsync(cancellationToken);

            if (fileTreeMap == null)
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var imageBrush = await CreateImageBrush(fileTreeMap, cancellationToken);

            if (visualHost != null)
            {
                visualHost.Fill = imageBrush;
            }
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
                        pixelsPerDip)
                    {
                        MaxTextWidth = item.RectangleDescription.Rectangle.Width,
                        MaxLineCount = 1
                    };

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
