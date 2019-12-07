using KombajnDoPracy.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KombajnDoPracy
{
    /// <summary>
    /// Interaction logic for DaysDataInne.xaml
    /// </summary>
    public partial class EditButtons : Window, INotifyPropertyChanged
    {
        public EditButtonViewModel EditButtonViewModel { get; set; }

        public EditButtons(SerializableButtonItemViewModel serializableVm)
        {
            InitializeComponent();

            EditButtonViewModel = EditButtonViewModel.GetFromSerializableButtonItemViewModel(serializableVm);
            this.DataContext = EditButtonViewModel;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Close();
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
