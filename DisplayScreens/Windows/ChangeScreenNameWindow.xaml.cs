using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DisplayScreens.Windows
{
    /// <summary>
    /// Interaction logic for ChangeScreenNameWindow.xaml
    /// </summary>
    public partial class ChangeScreenNameWindow : Window, INotifyPropertyChanged
    {
        private string text;
        private string fileName;
        public bool Result { get; set; }

        //Jesteś pewien, że chcesz wyłączyć komputer?
        public ChangeScreenNameWindow(string displayedName, string fileName)
        {
            InitializeComponent();

            this.Text = displayedName;
            this.FileName = System.IO.Path.GetFileName(fileName);
            this.DataContext = this;
            Keyboard.Focus(tbDisplayedName);
            tbDisplayedName.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbDisplayedName.SelectAll();
            //tbDisplayedName.CaretIndex = tbDisplayedName.Text.Length;
        }

        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = value;
                OnPropertyChanged("FileName");
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

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        private void tbFileName_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                ((TextBox)sender).SelectAll();
            }
        }

        private void tbDisplayedName_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Result = true;
                Close();
            }
        }
    }
}
