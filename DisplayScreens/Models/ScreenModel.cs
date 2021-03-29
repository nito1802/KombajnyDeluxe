using DisplayScreens.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DisplayScreens
{
    public class ScreenModel : INotifyPropertyChanged
    {
        private RelayCommand onMouseEnter;
        private RelayCommand onMouseLeave;
        private RelayCommand onMouseLeftButtonDown;
        private RelayCommand removeImage;

        public static Action<string> SetFullScreen { get; set; }
        public static Func<string, MessageBoxResult> MessageBoxShow { get; set; }
        public static Action<ScreenModel> RemoveImageFromCollection { get; set; }
        public static Func<string, Brush> GetBrushForTagFunc { get; set; }
        public static Action<string, string> SerializeSelectedTag { get; set; }

        public ObservableCollection<TagModel> Tags { get; set; } = new ObservableCollection<TagModel>();


        private DateTime creationDate;
        private double widthImg;
        private double heightImg;
        private string formatCreationDate;
        private bool runStoryboard;

        public ScreenModel() { }
        public ScreenModel(string fullName, double widthImg, double heightImg)
        {
            //Name = Path.GetFileName(fullName);
            Name = fullName;
            Description = Path.GetFileNameWithoutExtension(fullName).Replace('_', ':');
            Directory = Path.GetDirectoryName(fullName);
            CreationDate = File.GetCreationTime(fullName);

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
            var emptyTag = new TagModel() { Name = "Empty", BackgroundBrush = GetBrushForTagFunc("Empty") };

            Tags.Add(new TagModel() { Name = "FL Studio", BackgroundBrush = GetBrushForTagFunc("FL Studio") });
            Tags.Add(new TagModel() { Name = "Xamarin", BackgroundBrush = GetBrushForTagFunc("Xamarin") });
            Tags.Add(new TagModel() { Name = "ASP", BackgroundBrush = GetBrushForTagFunc("ASP") });
            Tags.Add(new TagModel() { Name = "Inne", BackgroundBrush = GetBrushForTagFunc("Inne") });
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

        public string Name { get; set; }
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
                        SetFullScreen?.Invoke(Name);
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
                        if(res == MessageBoxResult.Yes) RemoveImageFromCollection(this);
                    }
                     , param => true);
                }
                return removeImage;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged; //INotifyPropertyChanged

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
