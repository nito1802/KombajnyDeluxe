using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KombajnDoPracy.Models
{
    public class LinkUrlModel : ButtonFacade, INotifyPropertyChanged
    {
        private RelayCommand clickUrl;

        private string name;
        private string url;

        public static Action CloseApplication { get; set; }

        public LinkUrlModel(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                OnPropertyChanged(nameof(Url));
            }
        }

        [JsonIgnore]
        public ICommand ClickUrlCommand
        {
            get
            {
                if (clickUrl == null)
                {
                    clickUrl = new RelayCommand(param =>
                    {
                        Process.Start(Url);
                        CloseApplication();
                    }
                     , param => true);
                }
                return clickUrl;
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
