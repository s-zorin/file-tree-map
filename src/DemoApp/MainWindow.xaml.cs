using System.IO;
using System.Windows;

namespace DemoApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FileTreeMapControl.DirectoryPath))
            {
                return;
            }

            FileTreeMapControl.DirectoryPath = Directory.GetParent(FileTreeMapControl.DirectoryPath)?.FullName;
        }
    }
}
