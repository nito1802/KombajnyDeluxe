using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows;

namespace DisplayScreens.Converters
{
    public sealed class BitmapFrameConverter : IValueConverter
    {
        //doubles purely to facilitate easy data binding
        private double _decodePixelWidth;

        private double _decodePixelHeight;

        public double DecodePixelWidth
        {
            get
            {
                return _decodePixelWidth;
            }
            set
            {
                _decodePixelWidth = value;
            }
        }

        public double DecodePixelHeight
        {
            get
            {
                return _decodePixelHeight;
            }
            set
            {
                _decodePixelHeight = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = value as string;

            if (path != null)
            {
                //create new stream and create bitmap frame
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                string assemblyPath = string.Empty;

                switch (path)
                {
                    case "relativeFolderSprite":
                        assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(BitmapFrameConverter)).Location);
                        path = Path.Combine(assemblyPath, @"Sprites\Folders\FolderSprite.png");
                        break;

                    case "relativeFolderSpriteEmpty":
                        assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(BitmapFrameConverter)).Location);
                        path = Path.Combine(assemblyPath, @"Sprites\Folders\FolderSpriteEmpty.png");
                        break;

                    case "relativeVideoSprite":
                        assemblyPath = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(BitmapFrameConverter)).Location);
                        path = Path.Combine(assemblyPath, @"Sprites\VideoSprite.png");
                        break;

                    default:
                        break;
                }

                bitmapImage.StreamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
                bitmapImage.DecodePixelWidth = (int)_decodePixelWidth;
                bitmapImage.DecodePixelHeight = (int)_decodePixelHeight;
                //load the image now so we can immediately dispose of the stream
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                //clean up the stream to avoid file access exceptions when attempting to delete images
                bitmapImage.StreamSource.Dispose();

                return bitmapImage;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}