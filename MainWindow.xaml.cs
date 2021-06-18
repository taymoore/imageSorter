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
        private Random rand;
        private TransformedBitmap bitmapImage;
        private Dictionary<Key, string[]> keyMap = new Dictionary<Key, string[]>();

        public MainWindow()
        {
            InitializeComponent();
            KeyDown += OnKeyDown;
            AddKey("NumPad1", @"D:\wallpapers\0 - very nsfw", @"D:\Phone Wallpapers\unprocessed\photo\6-verynsfw");
            AddKey("NumPad2", @"D:\wallpapers\2 - nsfw", @"D:\Phone Wallpapers\unprocessed\photo\5-nsfw");
            AddKey("NumPad3", @"D:\wallpapers\3 - sorta nsfw", @"D:\Phone Wallpapers\unprocessed\photo\4-sortansfw");
            AddKey("NumPad4", @"D:\wallpapers\1 - hentai", @"D:\Phone Wallpapers\unprocessed\illust\6-verynsfw");
            AddKey("NumPad5", @"D:\wallpapers\2 - nsfw", @"D:\Phone Wallpapers\unprocessed\illust\5-nsfw");
            AddKey("NumPad6", @"D:\wallpapers\3 - sorta nsfw", @"D:\Phone Wallpapers\unprocessed\illust\4-sortansfw");
            Key key;
            if(Enum.TryParse("NumPad0", out key))
            {
                keyMap.Add(key, new string[] {"delete"});
            }
            if(Enum.TryParse("Enter", out key))
            {
                keyMap.Add(key, new string[] {"skip"});
            }
        }

        private void AddKey(string keyStr, string pathLandscape, string pathPortrait)
        {
            Key key;
            if(Enum.TryParse(keyStr, out key))
            {
                keyMap.Add(key, new string[] { pathLandscape, pathPortrait });
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
//            imageFileIndex = 0;
//            imageFile = imageFiles[imageFileIndex];
        }

        private void GetRandomImage()
        {
            imageFile = imageFiles[rand.Next(imageFiles.Count())];
        }

        private void LoadImage()
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imageFile.FullName);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmapImage = new TransformedBitmap(bitmap, new RotateTransform(GetRotation(imageFile.FullName)));
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

            if(keyMap[e.Key].Length == 1)
            {
                if(keyMap[e.Key][0] == "delete")
                {
                    DeleteImage();
                    return;
                }
                else if (keyMap[e.Key][0] == "skip")
                {
                    LoadNextImage();
                    return;
                }
            }

            if(keyMap[e.Key].Length != 2)
            {
                throw new Exception($"Keymap has wrong length of {keyMap[e.Key].Length}.");
            }

            bool isLandscape = bitmapImage.PixelWidth > bitmapImage.PixelHeight;
            string destPath = System.IO.Path.Combine(keyMap[e.Key][isLandscape ? 0 : 1], System.IO.Path.GetFileName(imageFile.FullName));
            int destPathUniqueIndex = 0;
            while(File.Exists(destPath))
            {
                destPathUniqueIndex++;
                destPath = System.IO.Path.Combine(keyMap[e.Key][isLandscape ? 0 : 1], $"{System.IO.Path.GetFileNameWithoutExtension(imageFile.FullName)} ({destPathUniqueIndex}){System.IO.Path.GetExtension(imageFile.FullName)}");
            }
            File.Move(imageFile.FullName, destPath);
            LoadNextImage();
        }

        private void DeleteImage()
        {
            string imageName = imageFile.FullName;
            LoadNextImage();
            File.Delete(imageName);
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
            if(bitmapImage.PixelHeight < 500 || bitmapImage.PixelWidth < 500)
            {
                DeleteImage();
            }
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
