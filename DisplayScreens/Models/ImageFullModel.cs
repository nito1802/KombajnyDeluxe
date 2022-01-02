using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;

namespace DisplayScreens.Models
{
    public class ImageFullModel : INotifyPropertyChanged
    {
        private ImageSource imgName;

        public ImageSource ImgName
        {
            get { return imgName; }
            set
            {
                imgName = value;
                OnPropertyChanged(nameof(ImgName));
            }
        }

        private string displayedName;

        public string DisplayedName
        {
            get { return displayedName; }
            set
            {
                displayedName = value;
                OnPropertyChanged(nameof(DisplayedName));
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
