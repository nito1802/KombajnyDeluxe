using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kombajn_Shortcut.Views
{
    /// <summary>
    /// Interaction logic for ScreenAnimationWindow.xaml
    /// </summary>
    public partial class ScreenAnimationWindow : Window
    {
        public double ScaleImg { get; set; } = 0.1;
        public double imgWidth { get; set; }
        public double imgHeight { get; set; }


        public ScreenAnimationWindow(string screenPath)
        {
            InitializeComponent();

            myImg.Source = BitmapFromUri(new Uri(screenPath));
        }

        public ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            imgWidth = bitmap.Width * ScaleImg;
            imgHeight = bitmap.Height * ScaleImg;

            return bitmap;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myImg.Width = imgWidth;
            myImg.Height = imgHeight;

            Storyboard sb = this.FindResource("sbAnimateImage") as Storyboard;
            //Storyboard.SetTarget(sb, );
            sb.Completed += Sb_Completed;
            sb.Begin();
        }

        private void Sb_Completed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
