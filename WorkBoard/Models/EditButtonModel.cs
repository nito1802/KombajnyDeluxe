using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows.Input;
using WorkBoard.Enums;
using WorkBoard.Models;

namespace KombajnDoPracy.ViewModels
{
    public class EditButtonModel : NormalButton, INotifyPropertyChanged
    {
        public static Action<EditButtonModel, EditButtonModel> InsertNewButtonAction { get; set; }
        public static Action<EditButtonModel> DeleteButtonAction { get; set; }
        public static Action<EditButtonModel, int> IdxButtonAction { get; set; }


        private RelayCommand insertButton;
        private RelayCommand deleteButton;
        private RelayCommand idxButtonUp;
        private RelayCommand idxButtonDown;

        private string path;
        private string name;
        private string description;
        private int idx;
        private long clickCounter;

        public EditButtonModel(string path, string name, string description, ButtonGroup groupId, bool canDelete, long clickCounter, string tagName)
        {
            this.Path = path;
            this.Name = name;
            this.Description = description;
            this.GroupId = groupId;
            this.CanDelete = canDelete;
            this.ClickCounter = clickCounter;
            this.TagName = tagName;
        }

        public EditButtonModel(ButtonGroup groupId)
        {
            this.GroupId = groupId;
            this.Name = "Name";
        }

        public new string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged("Path");
            }
        }
        public new string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public new string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }
        public new int Idx
        {
            get => idx;
            set
            {
                idx = value;
                OnPropertyChanged("Idx");
            }
        }
        public new long ClickCounter
        {
            get => clickCounter;
            set
            {
                clickCounter = value;
                OnPropertyChanged("ClickCounter");
            }
        }

        [JsonIgnore]
        public ICommand InsertButton
        {
            get
            {
                if (insertButton == null)
                {
                    insertButton = new RelayCommand(param =>
                    {
                        InsertNewButtonAction?.Invoke(this, new EditButtonModel(this.GroupId));
                    }
                     , param => true);
                }
                return insertButton;
            }
        }

        [JsonIgnore]
        public ICommand DeleteButton
        {
            get
            {
                if (deleteButton == null)
                {
                    deleteButton = new RelayCommand(param =>
                    {
                        DeleteButtonAction?.Invoke(this);
                    }
                     , param => true);
                }
                return deleteButton;
            }
        }

        [JsonIgnore]
        public ICommand IdxButtonUp
        {
            get
            {
                if (idxButtonUp == null)
                {
                    idxButtonUp = new RelayCommand(param =>
                    {
                        IdxButtonAction?.Invoke(this, 1);
                    }
                     , param => true);
                }
                return idxButtonUp;
            }
        }
        [JsonIgnore]
        public ICommand IdxButtonDown
        {
            get
            {
                if (idxButtonDown == null)
                {
                    idxButtonDown = new RelayCommand(param =>
                    {
                        IdxButtonAction?.Invoke(this, -1);
                    }
                     , param => true);
                }
                return idxButtonDown;
            }
        }

        public new ICommand OpenOrRestoreWindowCommand => null;

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
