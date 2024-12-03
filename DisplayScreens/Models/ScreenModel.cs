using DisplayScreens.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DisplayScreens
{
    public class ScreenModel : INotifyPropertyChanged
    {
        private RelayCommand onMouseEnter;
        private RelayCommand onMouseLeave;
        private RelayCommand onMouseLeftButtonDown;
        private RelayCommand removeImage;
        private RelayCommand renameImage;

        public static Action<string, string, string> SetFullScreen { get; set; }
        public static Func<string, MessageBoxResult> MessageBoxShow { get; set; }
        public static Action<ScreenModel> RemoveImageFromCollection { get; set; }
        public static Action<ScreenModel> RenameImageAction { get; set; }
        public static Func<string, Brush> GetBrushForTagFunc { get; set; }
        public static Action<string, string> SerializeSelectedTag { get; set; }
        public static Action<string, string> SerializeSelectedDisplayName { get; set; }

        public ObservableCollection<TagModel> Tags { get; set; } = new ObservableCollection<TagModel>();

        private DateTime creationDate;
        private DateTime modificationDate;
        private double widthImg;
        private double heightImg;
        private string formatCreationDate;
        private bool runStoryboard;

        public ScreenModel()
        { }

        public ScreenModel(string fullName, double widthImg, double heightImg)
        {
            //Name = Path.GetFileName(fullName);
            Name = fullName;
            Description = Path.GetFileNameWithoutExtension(fullName).Replace('_', ':');
            Directory = Path.GetDirectoryName(fullName);
            CreationDate = File.GetCreationTime(fullName);
            ModificationDate = File.GetLastWriteTime(fullName);

            WidthImg = widthImg;
            HeightImg = heightImg;
        }

        //SerializeSelectedTag

        private TagModel selectedTag;

        public TagModel SelectedTag
        {
            get { return selectedTag; }
            set
            {
                selectedTag = value;
                SerializeSelectedTag(Name, value.Name);
                OnPropertyChanged(nameof(SelectedTag));
            }
        }

        public void InitializeSelectedTag(TagModel tag)
        {
            selectedTag = tag;
            OnPropertyChanged(nameof(SelectedTag));
        }

        public void InitializeTags()
        {
            Tags.Clear();
            var emptyTag = new TagModel() { Name = "Empty", BackgroundBrush = GetBrushForTagFunc("Empty") };

            Tags.Add(new TagModel() { Name = "Android", BackgroundBrush = GetBrushForTagFunc("Android") });
            Tags.Add(new TagModel() { Name = "FL Studio", BackgroundBrush = GetBrushForTagFunc("FL Studio") });
            Tags.Add(new TagModel() { Name = "Jetpack Compose", BackgroundBrush = GetBrushForTagFunc("Jetpack Compose") });
            Tags.Add(new TagModel() { Name = "Xamarin", BackgroundBrush = GetBrushForTagFunc("Xamarin") });
            Tags.Add(new TagModel() { Name = "ASP", BackgroundBrush = GetBrushForTagFunc("ASP") });
            Tags.Add(new TagModel() { Name = "Unity", BackgroundBrush = GetBrushForTagFunc("Unity") });
            Tags.Add(emptyTag);

            selectedTag = emptyTag;
        }

        public void SetBitmap()
        {
            ButtonImage = BitmapFromUri(new Uri(Name));
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

        public async Task<ImageSource> BitmapFromUriAsync(Uri source)
        {
            var res = await Task.Run(() =>
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = source;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }).ConfigureAwait(false);

            return res;
        }

        public async Task SetBitmapAsync()
        {
            var bitmap = await BitmapFromUriAsync(new Uri(Name));

            if (Application.Current.Dispatcher.CheckAccess())
            {
                ButtonImage = bitmap;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => ButtonImage = bitmap);
            }

            //ButtonImage = bitmap;
            //Application.Current.Dispatcher.Invoke(() => ButtonImage = bitmap);
            //await Application.Current.Dispatcher.InvokeAsync(() => ButtonImage = bitmap).Wait();
        }

        private ImageSource buttonImage;

        public ImageSource ButtonImage
        {
            get { return buttonImage; }
            set
            {
                buttonImage = value;
                OnPropertyChanged(nameof(ButtonImage));
            }
        }

        private string buttonImageUri;

        public string ButtonImageUri
        {
            get { return buttonImageUri; }
            set
            {
                buttonImageUri = value;
                OnPropertyChanged(nameof(ButtonImageUri));
            }
        }

        public string Name { get; set; }

        private string displayedName;

        public string DisplayedName
        {
            get
            {
                return string.IsNullOrEmpty(displayedName) ? Description : displayedName;
            }
            set
            {
                displayedName = value;
                SerializeSelectedDisplayName(Name, displayedName);
                OnPropertyChanged(nameof(DisplayedName));
            }
        }

        public string Directory { get; set; }
        public string Description { get; set; }

        public DateTime CreationDate
        {
            get { return creationDate; }
            set
            {
                creationDate = value;
                FormatCreationDate = GetFormatCreationDate(creationDate);
            }
        }

        public DateTime ModificationDate
        {
            get { return modificationDate; }
            set
            {
                modificationDate = value;
                FormatCreationDate = GetFormatCreationDate(modificationDate);
            }
        }

        private string GetFormatCreationDate(DateTime date)
        {
            string result = date.ToString("dd MMMM yyyy");
            return result;
        }

        public double WidthImg
        {
            get { return widthImg; }
            set
            {
                widthImg = value;
                OnPropertyChanged(nameof(WidthImg));
            }
        }

        public double HeightImg
        {
            get { return heightImg; }
            set
            {
                heightImg = value;
                OnPropertyChanged(nameof(HeightImg));
            }
        }

        public bool RunStoryboard
        {
            get { return runStoryboard; }
            set
            {
                runStoryboard = value;
                OnPropertyChanged(nameof(RunStoryboard));
            }
        }

        public string FormatCreationDate
        {
            get { return formatCreationDate; }
            set
            {
                formatCreationDate = value;
                OnPropertyChanged(nameof(FormatCreationDate));
            }
        }

        public ICommand OnMouseEnter
        {
            get
            {
                if (onMouseEnter == null)
                {
                    onMouseEnter = new RelayCommand(param =>
                    {
                        //var sb = new Storyboard();

                        RunStoryboard = true;
                        //InsertSingleStrumAction?.Invoke(this);
                    }
                     , param => true);
                }
                return onMouseEnter;
            }
        }

        public ICommand OnMouseLeave
        {
            get
            {
                if (onMouseLeave == null)
                {
                    onMouseLeave = new RelayCommand(param =>
                    {
                        RunStoryboard = false;
                        //InsertSingleStrumAction?.Invoke(this);
                    }
                     , param => true);
                }
                return onMouseLeave;
            }
        }

        public ICommand OnMouseLeftButtonDown
        {
            get
            {
                if (onMouseLeftButtonDown == null)
                {
                    onMouseLeftButtonDown = new RelayCommand(param =>
                    {
                        SetFullScreen?.Invoke(Name, DisplayedName, SelectedTag.Name);
                        //InsertSingleStrumAction?.Invoke(this);
                    }
                     , param => true);
                }
                return onMouseLeftButtonDown;
            }
        }

        public ICommand RemoveImage
        {
            get
            {
                if (removeImage == null)
                {
                    removeImage = new RelayCommand(param =>
                    {
                        var res = MessageBoxShow?.Invoke("Czy na pewno chcesz usunąć screena?");
                        if (res == MessageBoxResult.Yes) RemoveImageFromCollection(this);
                    }
                     , param => true);
                }
                return removeImage;
            }
        }

        public ICommand RenameImage
        {
            get
            {
                if (renameImage == null)
                {
                    renameImage = new RelayCommand(param =>
                    {
                        RenameImageAction(this);
                    }
                     , param => true);
                }
                return renameImage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged; //INotifyPropertyChanged

        public void OnPropertyChangedMy()
        {
            OnPropertyChanged("SelectedTag");
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}