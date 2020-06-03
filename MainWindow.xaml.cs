using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
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
        private System.IO.FileInfo imageFile;
        private int imageFileIndex;
        private TransformedBitmap bitmapImage;
        private Dictionary<Key, string[]> keyMap = new Dictionary<Key, string[]>();

        public MainWindow()
        {
            InitializeComponent();
            KeyDown += OnKeyDown;
            Key key;
            if(Enum.TryParse("1", out key))
            {
                keyMap.Add(key, new string[] { @"C:\Users\Taylor\Desktop\landscape", @"C:\Users\Taylor\Desktop\portrait" });
            }
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
            imageFileIndex = 0;
            imageFile = imageFiles[imageFileIndex];
        }

        private void LoadImage()
        {
            bitmapImage = new TransformedBitmap(new BitmapImage(new Uri(imageFile.FullName)), new RotateTransform(GetRotation(imageFile.FullName)));
            image.Source = bitmapImage;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(!keyMap.ContainsKey(e.Key))
            {
                return;
            }

            if(image.Source == null)
            {
                return;
            }

            bool isLandscape = bitmapImage.PixelWidth > bitmapImage.PixelHeight;
            string destPath = System.IO.Path.Combine(keyMap[e.Key][isLandscape ? 0 : 1], System.IO.Path.GetFileName(imageFile.FullName));
            int destPathUniqueIndex = 0;
            while(File.Exists(destPath))
            {
                destPathUniqueIndex++;
                destPath = System.IO.Path.Combine(keyMap[e.Key][isLandscape ? 0 : 1], $"{System.IO.Path.GetFileNameWithoutExtension(imageFile.FullName)} ({destPathUniqueIndex}){System.IO.Path.GetExtension(imageFile.FullName)}");
            }
            File.Copy(imageFile.FullName, destPath);
            LoadNextImage();
        }

        private void LoadNextImage()
        {
            if(++imageFileIndex >= imageFiles.Count)
            {
                imageFile = null;
                image.Source = null;
                bitmapImage = null;
                return;
            }
            imageFile = imageFiles[imageFileIndex];
            LoadImage();
        }

        private const string _orientationQuery = "System.Photo.Orientation";

        private Double GetRotation(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BitmapFrame bitmapFrame = BitmapFrame.Create(fileStream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                BitmapMetadata bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;

                if ((bitmapMetadata != null) && (bitmapMetadata.ContainsQuery(_orientationQuery)))
                {
                    object o = bitmapMetadata.GetQuery(_orientationQuery);

                    if (o != null)
                    {
                        //refer to http://www.impulseadventure.com/photo/exif-orientation.html for details on orientation values
                        switch ((ushort)o)
                        {
                            case 6:
                                return 90D;
                            case 3:
                                return 180D;
                            case 8:
                                return 270D;
                        }
                    }
                }
                return 0D;
            }
        }
    }
}
