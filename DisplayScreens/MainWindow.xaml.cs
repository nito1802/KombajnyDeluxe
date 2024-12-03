﻿using DisplayScreens.Enums;
using DisplayScreens.Models;
using DisplayScreens.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public string StartupPath { get; }
        public bool IsDragging { get; set; }
        public Point StartDragPosition { get; set; }
        public double ScrollFactor { get; set; }
        public double ScrollFactorSettings { get; set; } = 18; //o ile ma byc dzielony wektor kierunku, aby otrzymac scrollFactor
        public ShowStatus ShowStatus { get; set; }
        public bool InitFilterFrom { get; set; } = true;

        public static Dictionary<string, string> TagStatesDict { get; set; }
        public static Dictionary<string, string> ScreenDisplayedNames { get; set; }

        public ImageFullModel ImageFull { get; set; } = new ImageFullModel();
        public ObservableCollection<ScreenModel> ScreenModels { get; set; } = new ObservableCollection<ScreenModel>();
        public Dictionary<string, ScreenModel> FullPathToScreenDict { get; set; } = new Dictionary<string, ScreenModel>();
        private Dictionary<string, ShowFilesFromDate> FilterFromDict { get; set; }

        public ObservableCollection<TagModel> TagsMain { get; set; } = new ObservableCollection<TagModel>();
        public List<TagModel> MultipleFilterTags { get; set; } = new List<TagModel>();

        public List<TagModel> ShowFromDates { get; set; } = new List<TagModel>();

        public static double Scale { get; set; } = 0.20;
        public ShowFilesFromDate ShowFilesFromDate { get; set; }

        public DispatcherTimer TimerDragging { get; set; } = new DispatcherTimer();

        public MainWindow(int displayCondition, string startupPath)
        {
            InitializeComponent();

            StartupPath = startupPath;

            this.DataContext = this;
            userCtrlFullScreenImg.DataContext = ImageFull;
            ShowFilesFromDate = (ShowFilesFromDate)displayCondition;
            //ShowFilesFromDate = ShowFilesFromDate.LastYear;
            SetActions();
        }

        private void InitItems()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            TagStatesDict = DeserializeTagsState() ?? new Dictionary<string, string>();
            ScreenDisplayedNames = DeserializeDisplayedNames() ?? new Dictionary<string, string>();

            Stopwatch pngSw = new Stopwatch();
            pngSw.Start();
            var allPng = Directory.GetFiles(StartupPath, "*.png", SearchOption.AllDirectories);
            pngSw.Stop();

            var pngFromScreensDir = allPng.Where(a => Directory.GetParent(a).Name == "Screeny").ToList();

            if (pngFromScreensDir == null && !pngFromScreensDir.Any())
            {
                MessageBox.Show("Brak screenów!");
                Close();
            }

            Stopwatch pngSw2 = new Stopwatch();
            pngSw2.Start();
            var screensDetails = pngFromScreensDir.Select(a => new ScreenModel(a, 1920 * Scale, 1080 * Scale))
                                                  .Where(c => FilterScreens(c.ModificationDate, ShowFilesFromDate))
                                                  .OrderBy(b => b.ModificationDate)
                                                  //.Take(20)
                                                  .Reverse()
                                                  .ToList();
            pngSw2.Stop();

            Stopwatch sw3 = new Stopwatch();
            sw3.Start();
            var mainTag = cbMaintTag.SelectedItem as TagModel;

            Stopwatch internalSw1 = Stopwatch.StartNew();
            if (mainTag != null && mainTag.Name != "Everything")
            {
                screensDetails = screensDetails
                    .Where(a =>
                    {
                        string tagName = TagStatesDict.ContainsKey(a.Name) ? TagStatesDict[a.Name] : null;

                        if (tagName != null)
                        {
                            return tagName == mainTag.Name;
                        }

                        return false;
                    })
                    .ToList();
            }
            internalSw1.Stop();

            Stopwatch internalSw2 = Stopwatch.StartNew();

            foreach (var item in screensDetails)
            {
                item.ButtonImageUri = item.Name;
                //item.SetBitmap();
            }

            //await Task.WhenAll(screensDetails.Select(item => item.SetBitmapAsync())).ConfigureAwait(false);

            internalSw2.Stop();

            Stopwatch internalSw3 = Stopwatch.StartNew();

            var grouped = screensDetails.GroupBy(a => a.Directory).ToList();

            foreach (var item in grouped)
            {
                foreach (var internalItem in item)
                {
                    if (ScreenDisplayedNames.ContainsKey(internalItem.Name))
                    {
                        internalItem.DisplayedName = ScreenDisplayedNames[internalItem.Name];
                    }

                    ScreenModels.Add(internalItem);
                    FullPathToScreenDict.Add(internalItem.Name, internalItem);
                }
            }
            internalSw3.Stop();
            Stopwatch internalSw4 = Stopwatch.StartNew();

            foreach (var item in ScreenModels)
            {
                item.InitializeTags();
            }

            foreach (var item in TagStatesDict)
            {
                if (!FullPathToScreenDict.ContainsKey(item.Key)) continue;

                FullPathToScreenDict[item.Key].InitializeSelectedTag(FullPathToScreenDict[item.Key].Tags.First(a => a.Name == item.Value));
            }
            internalSw4.Stop();

            sw3.Stop();

            sw.Stop();

            //ICollectionView view = CollectionViewSource.GetDefaultView(ScreenModels);
            //view.GroupDescriptions.Add(new PropertyGroupDescription("FormatCreationDate"));
            //mainListbox.ItemsSource = view;
        }

        private void InitComboboxes()
        {
            ShowFromDates.Add(new TagModel() { Name = "Last Day", BackgroundBrush = (Brush)this.FindResource("DateGradientKey") });
            ShowFromDates.Add(new TagModel() { Name = "Last Week", BackgroundBrush = (Brush)this.FindResource("DateGradientKey") });
            ShowFromDates.Add(new TagModel() { Name = "Last Two Weeks", BackgroundBrush = (Brush)this.FindResource("DateGradientKey") });
            ShowFromDates.Add(new TagModel() { Name = "Last Month", BackgroundBrush = (Brush)this.FindResource("DateGradientKey") });
            ShowFromDates.Add(new TagModel() { Name = "Last Year", BackgroundBrush = (Brush)this.FindResource("DateGradientKey") });
            ShowFromDates.Add(new TagModel() { Name = "Everything", BackgroundBrush = (Brush)this.FindResource("DateGradientKey") });

            TagsMain.Add(new TagModel() { Name = "Everything", BackgroundBrush = (Brush)this.FindResource("EverythingGradientKey") });
            TagsMain.Add(new TagModel() { Name = "Android", BackgroundBrush = (Brush)this.FindResource("AndroidGradientKey") });
            TagsMain.Add(new TagModel() { Name = "FL Studio", BackgroundBrush = (Brush)this.FindResource("FlStudioGradientKey") });
            TagsMain.Add(new TagModel() { Name = "Xamarin", BackgroundBrush = (Brush)this.FindResource("XamarinGradientKey") });
            TagsMain.Add(new TagModel() { Name = "ASP", BackgroundBrush = (Brush)this.FindResource("AspGradientKey") });
            TagsMain.Add(new TagModel() { Name = "Jetpack Compose", BackgroundBrush = (Brush)this.FindResource("JetpackComposeGradientKey") });
            TagsMain.Add(new TagModel() { Name = "Unity", BackgroundBrush = (Brush)this.FindResource("UnityGradientKey") });
            TagsMain.Add(new TagModel() { Name = "Empty", BackgroundBrush = (Brush)this.FindResource("EmptyGradientKey") });

            var emptyTag = new TagModel() { Name = "Empty", BackgroundBrush = (Brush)this.FindResource("EmptyGradientKey") };

            MultipleFilterTags.Add(emptyTag);
            MultipleFilterTags.Add(new TagModel() { Name = "Android", BackgroundBrush = (Brush)this.FindResource("AndroidGradientKey") });
            MultipleFilterTags.Add(new TagModel() { Name = "FL Studio", BackgroundBrush = (Brush)this.FindResource("FlStudioGradientKey") });
            MultipleFilterTags.Add(new TagModel() { Name = "Jetpack Compose", BackgroundBrush = (Brush)this.FindResource("JetpackComposeGradientKey") });
            MultipleFilterTags.Add(new TagModel() { Name = "Unity", BackgroundBrush = (Brush)this.FindResource("UnityGradientKey") });
            MultipleFilterTags.Add(new TagModel() { Name = "Xamarin", BackgroundBrush = (Brush)this.FindResource("XamarinGradientKey") });
            MultipleFilterTags.Add(new TagModel() { Name = "ASP", BackgroundBrush = (Brush)this.FindResource("AspGradientKey") });
        }

        private void SetActions()
        {
            ScreenModel.SetFullScreen = (source, displayedName, selectedTagName) =>
            {
                ImageFull.ImgName = BitmapFromUri(new Uri(source));
                ImageFull.DisplayedName = displayedName;
                if (selectedTagName != "Empty")
                {
                    ImageFull.DisplayedName += $" ({selectedTagName})";
                }
                ShowFullImage();
                userCtrlFullScreenImg.btnNext.Focus();
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

            ScreenModel.RenameImageAction = (image) =>
            {
                var changeVideoDisplayNameWindow = new ChangeScreenNameWindow(image.DisplayedName, image.Name);

                changeVideoDisplayNameWindow.Owner = this;
                changeVideoDisplayNameWindow.ShowDialog();

                if (changeVideoDisplayNameWindow.Result == true)
                {
                    image.DisplayedName = changeVideoDisplayNameWindow.Text;
                }
            };

            FilterFromDict = new Dictionary<string, ShowFilesFromDate>
            {
                {"Last Day", ShowFilesFromDate.LastDay },
                {"Last Week", ShowFilesFromDate.LastWeek },
                {"Last Two Weeks", ShowFilesFromDate.LastTwoWeeks },
                {"Last Month", ShowFilesFromDate.LastMonth },
                {"Last Year", ShowFilesFromDate.LastYear },
                {"Everything", ShowFilesFromDate.Everything }
            };

            Dictionary<string, string> TagNameToBrushKeyDict = new Dictionary<string, string>
            {
                {"Android", "AndroidGradientKey" },
                {"FL Studio", "FlStudioGradientKey" },
                {"Xamarin", "XamarinGradientKey" },
                {"ASP", "AspGradientKey" },
                {"Empty", "EmptyGradientKey" },
                {"Everything", "EverythingGradientKey" },
                {"Jetpack Compose", "JetpackComposeGradientKey" },
                {"Unity", "UnityGradientKey" },
            };

            ScreenModel.GetBrushForTagFunc = (key) =>
            {
                var brushKey = TagNameToBrushKeyDict[key];
                var brush = (Brush)this.FindResource(brushKey);
                return brush;
            };

            ScreenModel.SerializeSelectedTag = (fullPath, tagName) =>
            {
                if (TagStatesDict.ContainsKey(fullPath)) TagStatesDict[fullPath] = tagName;
                else TagStatesDict.Add(fullPath, tagName);

                SerializeTagsState();
            };

            ScreenModel.SerializeSelectedDisplayName = (fullPath, tagName) =>
            {
                if (ScreenDisplayedNames.ContainsKey(fullPath)) ScreenDisplayedNames[fullPath] = tagName;
                else ScreenDisplayedNames.Add(fullPath, tagName);

                SerializeDisplayedNames();
            };

            FullScreenImageControl.PreviousScreen = () =>
            {
                if (mainListbox.SelectedIndex == 0) return;

                mainListbox.SelectedIndex--;
                var selectedItem = mainListbox.SelectedItem as ScreenModel;

                ImageFull.ImgName = selectedItem.ButtonImage;
                ImageFull.DisplayedName = selectedItem.DisplayedName;
            };

            FullScreenImageControl.NextScreen = () =>
            {
                if (mainListbox.SelectedIndex == ScreenModels.Count - 1) return;

                mainListbox.SelectedIndex++;
                var selectedItem = mainListbox.SelectedItem as ScreenModel;

                ImageFull.ImgName = selectedItem.ButtonImage;
                ImageFull.DisplayedName = selectedItem.DisplayedName;
            };

            FullScreenImageControl.RemoveScreen = () =>
            {
                var selectedItem = mainListbox.SelectedItem as ScreenModel;

                if (selectedItem != null)
                {
                    var res = MessageBox.Show("Czy na pewno chcesz usunąć screena?", "Warning", MessageBoxButton.YesNo);

                    if (res == MessageBoxResult.No) return;

                    int removedIdx = mainListbox.SelectedIndex;
                    ScreenModel.RemoveImageFromCollection(selectedItem);

                    mainListbox.SelectedIndex = removedIdx;
                    var selectedItemNext = mainListbox.SelectedItem as ScreenModel;

                    ImageFull.ImgName = selectedItemNext.ButtonImage;
                    ImageFull.DisplayedName = selectedItem.DisplayedName;
                }
            };

            FullScreenImageControl.GoBack = () =>
            {
                HideFullImage();
                var listBoxItem = (ListBoxItem)mainListbox
                                                .ItemContainerGenerator
                                                .ContainerFromItem(mainListbox.SelectedItem);
                listBoxItem.Focus();
            };
        }

        private bool FilterScreens(DateTime date, ShowFilesFromDate condition)
        {
            if (condition == ShowFilesFromDate.LastDay)
            {
                var diff = DateTime.Now - date;
                return diff.TotalDays <= 1;
            }
            else if (condition == ShowFilesFromDate.LastWeek)
            {
                var diff = DateTime.Now - date;
                return diff.TotalDays <= 7;
            }
            else if (condition == ShowFilesFromDate.LastTwoWeeks)
            {
                var diff = DateTime.Now - date;
                return diff.TotalDays <= 14;
            }
            else if (condition == ShowFilesFromDate.LastMonth)
            {
                var diff = DateTime.Now - date;
                return diff.TotalDays <= 31;
            }
            else if (condition == ShowFilesFromDate.LastYear)
            {
                var diff = DateTime.Now - date;
                return diff.TotalDays <= 365;
            }
            else if (condition == ShowFilesFromDate.Everything)
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

        private void ShowFullImage()
        {
            userCtrlFullScreenImg.Visibility = Visibility.Visible;
            cbMaintTag.Visibility = Visibility.Hidden;
            mySv.Visibility = Visibility.Hidden;
            ShowStatus = ShowStatus.FullImage;
            stFilterFrom.Visibility = Visibility.Hidden;
            stMultipleFilter.Visibility = Visibility.Hidden;
            stMultipleRemove.Visibility = Visibility.Hidden;
            stMultipleRename.Visibility = Visibility.Hidden;
        }

        private void HideFullImage()
        {
            userCtrlFullScreenImg.Visibility = Visibility.Hidden;
            cbMaintTag.Visibility = Visibility.Visible;
            mySv.Visibility = Visibility.Visible;
            ShowStatus = ShowStatus.ListOfImages;
            stFilterFrom.Visibility = Visibility.Visible;
            stMultipleFilter.Visibility = Visibility.Hidden;
            stMultipleRemove.Visibility = Visibility.Hidden;
            stMultipleRename.Visibility = Visibility.Hidden;
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
            listBoxItem?.Focus();
            InitFilterFrom = false;

            InitItems();

            Stopwatch sw4 = Stopwatch.StartNew();
            ICollectionView view = CollectionViewSource.GetDefaultView(ScreenModels);
            view.GroupDescriptions.Add(new PropertyGroupDescription("FormatCreationDate"));
            mainListbox.ItemsSource = view;

            TimerDragging.Tick += (sender, e) =>
            {
                if (IsDragging)
                {
                    mySv.ScrollToVerticalOffset(mySv.VerticalOffset + ScrollFactor);
                }
            };

            TimerDragging.Interval = TimeSpan.FromMilliseconds(10);

            InitComboboxes();
            sw4.Stop();
        }

        private void mySv_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            mySv.ScrollToVerticalOffset(mySv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void fullImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton1 || e.ChangedButton == MouseButton.Right)
            {
                HideFullImage();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.WindowState = WindowState.Minimized;
            }
            else if (e.Key == Key.Enter)
            {
                var selectedItem = mainListbox.SelectedItem as ScreenModel;

                if (selectedItem != null)
                {
                    ImageFull.ImgName = BitmapFromUri(new Uri(selectedItem.Name));
                    ImageFull.DisplayedName = selectedItem.DisplayedName;

                    if (selectedItem.SelectedTag.Name != "Empty")
                    {
                        ImageFull.DisplayedName += $" ({selectedItem.SelectedTag.Name})";
                    }

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

        private void cbMaintTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InitFilterFrom) return;
            var changedTag = e.AddedItems[0] as TagModel;

            if (changedTag != null)
            {
                ScreenModels.Clear();
                FullPathToScreenDict.Clear();

                InitItems();
            }
        }

        private static void SerializeDictionary(Dictionary<string, string> dict, string fileName)
        {
            string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationFolder = "KombajnDeluxe Data";
            var appDataPath = System.IO.Path.Combine(myDocumentPath, applicationFolder);
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

            string serializedStatePath = System.IO.Path.Combine(appDataPath, fileName);

            var serializedContent = JsonConvert.SerializeObject(dict, Formatting.Indented);
            File.WriteAllText(serializedStatePath, serializedContent);
        }

        private static Dictionary<string, string> DeserializeTagsState(string fileName)
        {
            string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationFolder = "KombajnDeluxe Data";
            var appDataPath = System.IO.Path.Combine(myDocumentPath, applicationFolder);

            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
            string serializedStatePath = System.IO.Path.Combine(appDataPath, fileName);

            if (!File.Exists(serializedStatePath)) return null;

            var serializedContent = File.ReadAllText(serializedStatePath);
            var res = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedContent);

            return res;
        }

        private static void SerializeTagsState()
        {
            SerializeDictionary(TagStatesDict, "DisplayScreenTags.json");
        }

        private static Dictionary<string, string> DeserializeTagsState()
        {
            return DeserializeTagsState("DisplayScreenTags.json");
        }

        private static void SerializeDisplayedNames()
        {
            SerializeDictionary(ScreenDisplayedNames, "ScreenDisplayedNames.json");
        }

        private static Dictionary<string, string> DeserializeDisplayedNames()
        {
            return DeserializeTagsState("ScreenDisplayedNames.json");
        }

        private void cbFilterFromDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InitFilterFrom) return;
            var changedTag = e.AddedItems[0] as TagModel;

            ShowFilesFromDate = FilterFromDict[changedTag.Name];

            FullPathToScreenDict.Clear();
            ScreenModels.Clear();

            InitItems();
        }

        private void cbSetFilterMultiple_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InitFilterFrom) return;
            var changedTag = e.AddedItems[0] as TagModel;

            foreach (var item in mainListbox.SelectedItems)
            {
                var screenModel = (ScreenModel)item;
                screenModel.SelectedTag = changedTag;
            }

            foreach (var item in ScreenModels)
            {
                item.InitializeTags();
            }

            foreach (var item in TagStatesDict)
            {
                if (!FullPathToScreenDict.ContainsKey(item.Key)) continue;

                var myIdx = item.Value;
                FullPathToScreenDict[item.Key].InitializeSelectedTag(FullPathToScreenDict[item.Key].Tags.First(a => a.Name == myIdx));
            }
        }

        private void mainListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainListbox.SelectedItems.Count > 1)
            {
                stMultipleFilter.Visibility = Visibility.Visible;
                stMultipleRemove.Visibility = Visibility.Visible;
                stMultipleRename.Visibility = Visibility.Visible;
            }
            else
            {
                stMultipleFilter.Visibility = Visibility.Hidden;
                stMultipleRemove.Visibility = Visibility.Hidden;
                stMultipleRename.Visibility = Visibility.Hidden;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            FullPathToScreenDict.Clear();
            ScreenModels.Clear();

            InitItems();
        }

        private void btnRemoveMany_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Czy na pewno chcesz usunąć screeny?", "Warning", MessageBoxButton.YesNo);

            if (res == MessageBoxResult.No) return;

            //int removedIdx = mainListbox.SelectedIndex;

            var toRemove = mainListbox.SelectedItems.Cast<ScreenModel>().ToList();
            for (int i = 0; i < toRemove.Count; i++)
            {
                ScreenModel.RemoveImageFromCollection(toRemove[i]);
            }
        }

        private void btnRenameMany_Click(object sender, RoutedEventArgs e)
        {
            var toRename = mainListbox.SelectedItems
                .Cast<ScreenModel>()
                .OrderBy(a => a.Name)
                .ToList();

            var changeVideoDisplayNameWindow = new ChangeScreenNameWindow("", $"Count: {toRename.Count}");

            changeVideoDisplayNameWindow.Owner = this;
            changeVideoDisplayNameWindow.ShowDialog();

            if (changeVideoDisplayNameWindow.Result == true)
            {
                for (int i = 0; i < toRename.Count; i++)
                {
                    var item = toRename[i];

                    item.DisplayedName = $"{changeVideoDisplayNameWindow.Text} - Pt. {i + 1}";
                }
            }
        }
    }
}