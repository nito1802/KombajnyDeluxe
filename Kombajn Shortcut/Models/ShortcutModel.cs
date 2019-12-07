using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombajn_Shortcut.Models
{
    public enum ModificatorNumeration
    {
        None, Alt, Shift, Ctrl
    }

    public class ShortcutModel
    {
        private string name;
        private string describes;
        private bool allModifs;
        private ModificatorNumeration selectedModificator;
        private ModificatorNumeration alternateSelectedModificator;
        private string keyCode;
        public Action actionOnClick;
        public Action alternateActionOnClick;
        public Action actionOnSettings;

        public ShortcutModel()
        {
            Modificators = new List<ModificatorNumeration>();

            foreach (var item in Enum.GetValues(typeof(ModificatorNumeration)))
            {
                Modificators.Add((ModificatorNumeration)item);
            }
        }

        public List<ModificatorNumeration> Modificators { get; set; }



        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Describes
        {
            get
            {
                return describes;
            }

            set
            {
                describes = value;
                OnPropertyChanged("Describes");
            }
        }

        public string KeyCode
        {
            get
            {
                return keyCode;
            }

            set
            {
                keyCode = value;
                OnPropertyChanged("KeyCode");
            }
        }

        public ModificatorNumeration SelectedModificator
        {
            get
            {
                return selectedModificator;
            }

            set
            {
                selectedModificator = value;
                OnPropertyChanged("SelectedModificator");
            }
        }

        public ModificatorNumeration AlternateSelectedModificator
        {
            get
            {
                return alternateSelectedModificator;
            }

            set
            {
                alternateSelectedModificator = value;
                OnPropertyChanged("AlternateSelectedModificator");
            }
        }

        public bool AllModifs
        {
            get
            {
                return allModifs;
            }

            set
            {
                allModifs = value;
                OnPropertyChanged("AllModifs");
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
