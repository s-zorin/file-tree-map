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

        private void PathTextBox_Focus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholderTextVisibility();
        }

        private void PathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdatePlaceholderTextVisibility();
        }

        private void UpdatePlaceholderTextVisibility()
        {
            if (PathTextBox.IsFocused)
            {
                PlaceholderText.Visibility = Visibility.Hidden;
            }
            else
            {
                PlaceholderText.Visibility = string.IsNullOrEmpty(PathTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            
            if (dialog.ShowDialog() == true)
            {
                PathTextBox.Text = dialog.SelectedPath;
            }
        }
    }
}
