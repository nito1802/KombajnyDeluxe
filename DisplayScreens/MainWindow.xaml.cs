using DisplayScreens.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DisplayScreens
{
    public enum ShowStatus
    {
        ListOfImages,
        FullImage
    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public bool IsDragging { get; set; }
        public Point StartDragPosition { get; set; }
        public double ScrollFactor { get; set; }
        public double ScrollFactorSettings { get; set; } = 18; //o ile ma byc dzielony wektor kierunku, aby otrzymac scrollFactor
        public ShowStatus ShowStatus { get; set; }

        public ImageFullModel ImageFull { get; set; } = new ImageFullModel();
        public ObservableCollection<ScreenModel> ScreenModels { get; set; } = new ObservableCollection<ScreenModel>();

        public static double Scale { get; set; } = 0.20;

        public DispatcherTimer TimerDragging { get; set; } = new DispatcherTimer();
        public MainWindow(int displayCondition, string startupPath)
        {
            InitializeComponent();

            this.DataContext = this;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var allPng = Directory.GetFiles(startupPath, "*.png", SearchOption.AllDirectories);

            var pngFromScreensDir = allPng.Where(a => Directory.GetParent(a).Name == "Screeny").ToList();

            if (pngFromScreensDir == null && !pngFromScreensDir.Any())
            {
                MessageBox.Show("Brak screenów!");
                Close();
            }

            var screensDetails = pngFromScreensDir.Select(a => new ScreenModel(a, 1920 * Scale, 1080 * Scale))
                                                  .Where(c => FilterScreens(c.CreationDate, displayCondition))
                                                  .OrderBy(b => b.CreationDate)
                                                  //.Take(20)
                                                  .Reverse()
                                                  .ToList();

            foreach (var item in screensDetails)
            {
                item.SetBitmap();
            }
           
            var grouped = screensDetails.GroupBy(a => a.Directory).ToList();


            foreach (var item in grouped)
            {
                foreach (var internalItem in item.Reverse())
                {
                    ScreenModels.Add(internalItem);
                }
            }
            sw.Stop();

            TimerDragging.Tick += (sender, e) =>
            {
                if (IsDragging)
                {
                    mySv.ScrollToVerticalOffset(mySv.VerticalOffset + ScrollFactor);
                }
            };


            TimerDragging.Interval = TimeSpan.FromMilliseconds(10);

            ICollectionView view = CollectionViewSource.GetDefaultView(ScreenModels);
            view.GroupDescriptions.Add(new PropertyGroupDescription("FormatCreationDate"));
            mainListbox.ItemsSource = view;

            ScreenModel.SetFullScreen = (source) =>
            {
                ImageFull.ImgName = BitmapFromUri(new Uri(source)); ;
                ShowFullImage();
            };

            ScreenModel.MessageBoxShow = (text) =>
            {
                return MessageBox.Show(text, "Warning", MessageBoxButton.YesNo);
            };

            ScreenModel.RemoveImageFromCollection = (image) =>
            {
                ScreenModels.Remove(image);
                image.ButtonImage.Freeze();
                ImageFull.ImgName?.Freeze();
                File.Delete(image.Name);
            };
        }

        private bool FilterScreens(DateTime date, int condition)
        { 
            if(condition == 2)
            {
                var diff = DateTime.Now - date;
                return diff.TotalDays <= 14;
            }
            else if (condition == 4)
            {
                var diff = DateTime.Now - date;
                return diff.TotalDays <= 31;
            }
            else if (condition == 12)
            {
                var diff = DateTime.Now - date;
                return diff.TotalDays <= 365;
            }
            else if(condition == -1)
            {
                return true;
            }
            return false;

        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                StartDragPosition = e.GetPosition(this);
                IsDragging = true;
                TimerDragging.Start();
            }
        }

        private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                IsDragging = false;
                TimerDragging.Stop();
            }
        }

        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDragging)
            {
                var mousePos = e.GetPosition(this);
                var directionVector = mousePos - StartDragPosition;
                ScrollFactor = directionVector.Y / ScrollFactorSettings;
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnBackFromFull_Click(object sender, RoutedEventArgs e)
        {
            HideFullImage();
            var listBoxItem = (ListBoxItem)mainListbox
                                            .ItemContainerGenerator
                                            .ContainerFromItem(mainListbox.SelectedItem);
            listBoxItem.Focus();
        }

        private void ShowFullImage()
        {
            fullImage.Visibility = Visibility.Visible;
            btnBackFromFull.Visibility = Visibility.Visible;
            btnRemoveImageFullScreen.Visibility = Visibility.Visible;
            btnsMinimizeQuit.Visibility = Visibility.Hidden;
            mySv.Visibility = Visibility.Hidden;
            ShowStatus = ShowStatus.FullImage;
        }

        private void HideFullImage()
        {
            fullImage.Visibility = Visibility.Hidden;
            btnBackFromFull.Visibility = Visibility.Hidden;
            btnRemoveImageFullScreen.Visibility = Visibility.Hidden;
            btnsMinimizeQuit.Visibility = Visibility.Visible;
            mySv.Visibility = Visibility.Visible;
            ShowStatus = ShowStatus.ListOfImages;
        }

        private void mySv_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int myDelta = e.Delta;
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
            mainListbox.SelectedIndex = 0;
            var listBoxItem = (ListBoxItem)mainListbox
                                            .ItemContainerGenerator
                                            .ContainerFromItem(mainListbox.SelectedItem);
            listBoxItem.Focus();
        }

        private void mySv_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            mySv.ScrollToVerticalOffset(mySv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void fullImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton1)
            {
                HideFullImage();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (ShowStatus == ShowStatus.ListOfImages)
                {
                    this.WindowState = WindowState.Minimized;
                    //this.Close();
                }
                else if (ShowStatus == ShowStatus.FullImage)
                {
                    HideFullImage();
                    //mainListbox.Focus();
                    //Keyboard.Focus(mainListbox);

                    var listBoxItem = (ListBoxItem)mainListbox
                                                     .ItemContainerGenerator
                                                     .ContainerFromItem(mainListbox.SelectedItem);

                    listBoxItem.Focus();
                }
            }
            else if (e.Key == Key.Enter)
            {
                var selectedItem = mainListbox.SelectedItem as ScreenModel;

                if (selectedItem != null)
                {
                    ImageFull.ImgName = BitmapFromUri(new Uri(selectedItem.Name));
                    ShowFullImage();
                }
            }
        }

        public ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>
                    (v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void btnRemoveImageFullScreen_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = mainListbox.SelectedItem as ScreenModel;

            if(selectedItem != null)
            {
                var res = MessageBox.Show("Czy na pewno chcesz usunąć screena?", "Warning", MessageBoxButton.YesNo);

                if (res == MessageBoxResult.No) return;

                int removedIdx = mainListbox.SelectedIndex;
                ScreenModel.RemoveImageFromCollection(selectedItem);

                HideFullImage();

                mainListbox.SelectedIndex = removedIdx;
                var listBoxItem = (ListBoxItem)mainListbox
                                                 .ItemContainerGenerator
                                                 .ContainerFromItem(mainListbox.SelectedItem);

                listBoxItem.Focus();
            }
        }
    }

    public static class ScreenModelGenerator
    {
        public static ScreenModel GetScreenModel(string path)
        {
            return new ScreenModel("5 - 17_19_03.png", 1920 * MainWindow.Scale, 1080 * MainWindow.Scale);
        }
    }
}
