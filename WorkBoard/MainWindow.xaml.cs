using Common;
using KombajnDoPracy.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WorkBoard.Models;
using WorkBoard.Models.SpecialButtons;

namespace KombajnDoPracy
{

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public SerializableButtonItemViewModel ButtonsViewModel { get; set; }

        public HistoryWindowViewModel HistoryWindowViewModel { get; set; }

        private string leftGroupName;
        private string middleGroupName;
        private string rightGroupName;

        public MainWindow()
        {
            InitializeComponent();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string mojeDanePath = KombajnCommon.MojeDanePath;
            HistoryWindowViewModel = new HistoryWindowViewModel(mojeDanePath);

            this.DataContext = HistoryWindowViewModel;

            ButtonsViewModel = SerializableButtonItemViewModel.Load();
            mainGrid.DataContext = ButtonsViewModel;

            NormalButton.CloseApplication = () =>
            {
                ButtonsViewModel.WholeClickCount++;
                SerializableButtonItemViewModel.Save(ButtonsViewModel);
                WindowState = WindowState.Minimized;
            };

            SpecialDataButton.CloseApplication = () =>
            {
                ButtonsViewModel.WholeClickCount++;
                SerializableButtonItemViewModel.Save(ButtonsViewModel);
                WindowState = WindowState.Minimized;
            };
            SpecialNotesButton.CloseApplication = () =>
            {
                ButtonsViewModel.WholeClickCount++;
                SerializableButtonItemViewModel.Save(ButtonsViewModel);
                WindowState = WindowState.Minimized;
            };
        }

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

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();

            var processes = Process.GetProcesses();

            var currentProcess = Process.GetCurrentProcess();


            foreach (var item in processes)
            {
                if (item.ProcessName.Contains("KombajnDoPracy") && item.Id != currentProcess.Id)
                {
                    item.Kill();
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void BtnOptions_Click(object sender, RoutedEventArgs e)
        {
            EditButtons editButtonsWindow = new EditButtons(ButtonsViewModel);
            editButtonsWindow.ShowDialog();

            if(editButtonsWindow.DialogResult == true)
            {
                ButtonsViewModel = SerializableButtonItemViewModel.GetFromEditButtonViewModel(editButtonsWindow.EditButtonViewModel);
                SerializableButtonItemViewModel.Save(ButtonsViewModel);
                mainGrid.DataContext = ButtonsViewModel;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                WindowState = WindowState.Minimized;
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