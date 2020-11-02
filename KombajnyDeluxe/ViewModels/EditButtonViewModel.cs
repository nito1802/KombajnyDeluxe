using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KombajnDoPracy.ViewModels
{
    public class EditButtonViewModel : INotifyPropertyChanged
    {
        private string leftGroupName;
        private string middleGroupName;
        private string rightGroupName;

        public EditButtonViewModel()
        {
            InitActions();
        }

        public ObservableCollection<EditButtonModel> LeftButtons { get; set; } = new ObservableCollection<EditButtonModel>();
        public ObservableCollection<EditButtonModel> MiddleButtons { get; set; } = new ObservableCollection<EditButtonModel>();
        public ObservableCollection<EditButtonModel> RightButtons { get; set; } = new ObservableCollection<EditButtonModel>();
        public ObservableCollection<EditButtonModel> UrlButtons { get; set; } = new ObservableCollection<EditButtonModel>();
        public long WholeClickCount { get; set; }

        public string LeftGroupName
        {
            get => leftGroupName;
            set
            {
                leftGroupName = value;
                OnPropertyChanged("LeftGroupName");
            }
        }
        public string MiddleGroupName
        {
            get => middleGroupName;
            set
            {
                middleGroupName = value;
                OnPropertyChanged("MiddleGroupName");
            }
        }
        public string RightGroupName
        {
            get => rightGroupName;
            set
            {
                rightGroupName = value;
                OnPropertyChanged("RightGroupName");
            }
        }

        ObservableCollection<EditButtonModel> GetButtonCollection(int groupId)
        {
            ObservableCollection<EditButtonModel> res = null;
            if (groupId == 1)
            {
                res = LeftButtons;
            }
            else if (groupId == 2)
            {
                res = MiddleButtons;
            }
            else if (groupId == 3)
            {
                res = RightButtons;
            }
            else if (groupId == 4)
            {
                res = UrlButtons;
            }

            return res;
        }

        public void InitActions()
        {
            EditButtonModel.InsertNewButtonAction = (sourceButton, insertedButton) =>
            {
                var sourceButtons = GetButtonCollection(sourceButton.GroupId);

                var myBtn = sourceButtons.First(a => a.Name == sourceButton.Name && a.Path == sourceButton.Path && a.Description == sourceButton.Description && a.TagName == sourceButton.TagName);
                int myBtnIdx = sourceButtons.IndexOf(myBtn);

                sourceButtons.Insert(myBtnIdx + 1, insertedButton);

                int startIdx = 1;
                foreach (var item in sourceButtons)
                {
                    item.Idx = startIdx++;
                }
            };

            EditButtonModel.DeleteButtonAction = (button) =>
            {
                var sourceButtons = GetButtonCollection(button.GroupId);

                var myBtn = sourceButtons.First(a => a.Name == button.Name && a.Path == button.Path && a.Description == button.Description && a.TagName == button.TagName);

                sourceButtons.Remove(myBtn);

                if (!sourceButtons.Any(a => a.CanDelete)) sourceButtons.Add(new EditButtonModel(myBtn.GroupId));

                int startIdx = 1;
                foreach (var item in sourceButtons)
                {
                    item.Idx = startIdx++;
                }
            };

            EditButtonModel.IdxButtonAction = (button, idx) =>
            {
                var sourceButtons = GetButtonCollection(button.GroupId);

                var myBtn = sourceButtons.First(a => a.Name == button.Name && a.Path == button.Path && a.Description == button.Description && a.TagName == button.TagName);
                int myBtnIdx = sourceButtons.IndexOf(myBtn);
                int newIdx = myBtnIdx + idx;

                if (newIdx >= sourceButtons.Count || newIdx < 0) return;

                sourceButtons.Remove(myBtn);
                sourceButtons.Remove(myBtn);
                sourceButtons.Insert(newIdx, myBtn);

                int startIdx = 1;
                foreach (var item in sourceButtons)
                {
                    item.Idx = startIdx++;
                }
            };
        }

        public static EditButtonViewModel GetFromSerializableButtonItemViewModel(SerializableButtonItemViewModel serializableVm)
        {
            EditButtonViewModel res = new EditButtonViewModel();

            res.LeftGroupName = serializableVm.LeftGroupName;
            res.MiddleGroupName = serializableVm.MiddleGroupName;
            res.RightGroupName = serializableVm.RightGroupName;

            res.WholeClickCount = serializableVm.WholeClickCount;

            serializableVm.LeftButtons.ForEach(b => res.LeftButtons.Add(new EditButtonModel(b.Path, b.Name, b.Description, b.GroupId, b.CanDelete, b.ClickCounter, b.TagName)));
            serializableVm.MiddleButtons.ForEach(b => res.MiddleButtons.Add(new EditButtonModel(b.Path, b.Name, b.Description, b.GroupId, b.CanDelete, b.ClickCounter, b.TagName)));
            serializableVm.RightButtons.ForEach(b => res.RightButtons.Add(new EditButtonModel(b.Path, b.Name, b.Description, b.GroupId, b.CanDelete, b.ClickCounter, b.TagName)));
            serializableVm.LinkButtons.ForEach(b => res.UrlButtons.Add(new EditButtonModel(b.Path, b.Name, b.Description, b.GroupId, b.CanDelete, b.ClickCounter, b.TagName)));

            //jesli brak buttonow w grupie, wtedy dajemy jeden placeholder
            if (!res.LeftButtons.Any(a => a.CanDelete)) res.LeftButtons.Add(new EditButtonModel(1));
            if (!res.MiddleButtons.Any(a => a.CanDelete)) res.MiddleButtons.Add(new EditButtonModel(2));
            if (!res.RightButtons.Any(a => a.CanDelete)) res.RightButtons.Add(new EditButtonModel(3));
            if (!res.UrlButtons.Any(a => a.CanDelete)) res.UrlButtons.Add(new EditButtonModel(4));

            return res;
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
