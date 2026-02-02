using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Common;
using KombajnDoPracy.ViewModels;
using WorkBoard.Models;
using WorkBoard.Models.ClickableButttons.Details;
using WorkBoard.Models.SpecialButtons;

namespace KombajnDoPracy
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public SerializableButtonItemViewModel ButtonsViewModel { get; set; }

        public HistoryWindowViewModel HistoryWindowViewModel { get; set; }

        private string leftGroupName;
        private string middleGroupName;
        private string rightGroupName;

        public MainWindow()
        {
            InitializeComponent();

            string mojeDanePath = KombajnCommon.MojeDanePath;
            HistoryWindowViewModel = new HistoryWindowViewModel(mojeDanePath);
            DataContext = HistoryWindowViewModel;

            ButtonsViewModel = SerializableButtonItemViewModel.Load();
            Find<Grid>("MainGrid").DataContext = ButtonsViewModel;

            NormalButton.CloseApplication = CloseToTray;
            SpecialDataButton.CloseApplication = CloseToTray;
            SpecialNotesButton.CloseApplication = CloseToTray;
        }

        private void CloseToTray()
        {
            ButtonsViewModel.WholeClickCount++;
            SerializableButtonItemViewModel.Save(ButtonsViewModel);
            WindowState = WindowState.Minimized;
        }

        public string LeftGroupName
        {
            get => leftGroupName;
            set
            {
                leftGroupName = value;
                OnPropertyChanged(nameof(LeftGroupName));
            }
        }
        public string MiddleGroupName
        {
            get => middleGroupName;
            set
            {
                middleGroupName = value;
                OnPropertyChanged(nameof(MiddleGroupName));
            }
        }
        public string RightGroupName
        {
            get => rightGroupName;
            set
            {
                rightGroupName = value;
                OnPropertyChanged(nameof(RightGroupName));
            }
        }

        private void OptionsButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var editButtons = new Views.EditButtons(ButtonsViewModel);
            editButtons.ShowDialog(this);

            if (editButtons.DialogResult == true)
            {
                ButtonsViewModel = SerializableButtonItemViewModel.GetFromEditButtonViewModel(editButtons.EditButtonViewModel);
                SerializableButtonItemViewModel.Save(ButtonsViewModel);
                Find<Grid>("MainGrid").DataContext = ButtonsViewModel;
            }
        }

        private void Window_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void Window_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
            else if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
            {
                WindowState = WindowState.Minimized;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
