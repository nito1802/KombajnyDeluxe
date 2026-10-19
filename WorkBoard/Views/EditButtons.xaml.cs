using KombajnDoPracy.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using WorkBoard.Consts;

namespace KombajnDoPracy
{
    /// <summary>
    /// Interaction logic for DaysDataInne.xaml
    /// </summary>
    public partial class EditButtons : Window, INotifyPropertyChanged
    {
        public EditButtonViewModel EditButtonViewModel { get; set; }
        private string OldUrlButtonState { get; set; }
        public EditButtons(SerializableButtonItemViewModel serializableVm)
        {
            InitializeComponent();

            EditButtonViewModel = EditButtonViewModel.GetFromSerializableButtonItemViewModel(serializableVm);
            OldUrlButtonState = EditButtonViewModel.GetUrlButtonsState();
            this.DataContext = EditButtonViewModel;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            var urlButtonsState = EditButtonViewModel.GetUrlButtonsState();
            var date = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
            var changesLogPath = GetChangesLogFile();

            var sb = new StringBuilder();
            sb.AppendLine($"Stan linków na: {date}");
            sb.Append(PrintButtonUrlState(OldUrlButtonState, "OLD"));
            sb.Append(PrintButtonUrlState(urlButtonsState, "NEW"));
            var wholeText = sb.ToString();
            File.AppendAllText(changesLogPath, wholeText);

            DialogResult = true;
            Close();
        }

        private static string PrintButtonUrlState(string urlButtonsState, string tag)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{tag}>>>>");
            sb.AppendLine(urlButtonsState);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("---------------------------");
            return sb.ToString();
        }

        private static string GetChangesLogFile()
        {
            string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appDataPath = System.IO.Path.Combine(myDocumentPath, Consts.DataDirectoryName);

            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
            var changesLogPath = System.IO.Path.Combine(appDataPath, Consts.ChangesLogFileName);
            if (!File.Exists(changesLogPath)) File.Create(changesLogPath).Dispose();
            return changesLogPath;
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
