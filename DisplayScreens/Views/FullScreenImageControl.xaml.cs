using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DisplayScreens
{
    /// <summary>
    /// Interaction logic for FullScreenImageControl.xaml
    /// </summary>
    public partial class FullScreenImageControl : UserControl
    {
        public static Action GoBack { get; set; }
        public static Action RemoveScreen { get; set; }

        public static Action PreviousScreen { get; set; }
        public static Action NextScreen { get; set; }


        public ImageSource ImageSourcePath
        {
            get { return (ImageSource)GetValue(ImageSourcePathProperty); }
            set 
            { 
                SetValue(ImageSourcePathProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ImageSourcePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourcePathProperty =
            DependencyProperty.Register("ImageSourcePath", typeof(ImageSource), typeof(FullScreenImageControl), new PropertyMetadata(default(ImageSource), CheckpointsDetailsChanged));

        private static void CheckpointsDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FullScreenImageControl UserControl1Control = d as FullScreenImageControl;

            if (UserControl1Control != null) UserControl1Control.CheckpointsDetailsNonStaticChanged(e);

        }

        public void CheckpointsDetailsNonStaticChanged(DependencyPropertyChangedEventArgs e)
        {
            var myImgSource = e.NewValue as ImageSource;

            if (myImgSource != null) imgUserControl.Source = myImgSource;
            //tbTitleScreen.Text = "15 sierpnia 2 555";
            //imgUserControl.Source = ImageSourcePath;

        }


        public FullScreenImageControl()
        {
            InitializeComponent();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            PreviousScreen();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NextScreen();
        }

        private void btnBackFromFull_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void btnRemoveImageFullScreen_Click(object sender, RoutedEventArgs e)
        {
            RemoveScreen();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) PreviousScreen();
            else if (e.Key == Key.Right) NextScreen();
            else if (e.Key == Key.Delete) RemoveScreen();
            else if (e.Key == Key.Escape) GoBack();
        }
    }
}
