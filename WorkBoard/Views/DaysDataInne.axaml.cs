using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using KombajnDoPracy;

namespace KombajnDoPracy.Views
{
    public partial class DaysDataInne : Window
    {
        public DaysDataInne(List<DayDataModel> dayDataModels)
        {
            InitializeComponent();
            DataContext = dayDataModels;
            var listBox = this.FindControl<ListBox>("DataList");
            listBox.Items = dayDataModels;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
