using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

namespace imageSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string selectedPath;
        private System.Collections.Generic.List<System.IO.FileInfo> imageFiles;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnMenuOpenClicked(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if(dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    selectedPath = string.Empty;
                    return;
                }
                selectedPath = dialog.SelectedPath;
            }
            GetImageList();
            LoadImage();
        }

        private void GetImageList()
        {
            var validExtensions = new List<string> { "bmp", "jpg", "jpeg", "png", "gif" };
            var dirInfo = new DirectoryInfo(selectedPath);
            imageFiles = (from file in dirInfo.GetFiles("*.*", SearchOption.AllDirectories) where validExtensions.Contains(file.Extension.Replace(".", "").ToLower()) select new FileInfo(file.FullName)).ToList();
        }

        private void LoadImage()
        {
            image.Source = new BitmapImage(new Uri(imageFiles[0].FullName));
        }
    }
}
