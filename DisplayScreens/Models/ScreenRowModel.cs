using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DisplayScreens
{
    public class ScreenRowModel
    {
        public ObservableCollection<ScreenModel> ScreenModels { get; set; } = new ObservableCollection<ScreenModel>();
        public DateTime Date { get; set; }

        public ScreenRowModel(List<ScreenModel> screenModels, DateTime date)
        {
            screenModels.ForEach(a => ScreenModels.Add(a));
            Date = date;
        }

    }
}
