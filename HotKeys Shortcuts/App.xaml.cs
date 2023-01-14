using Common;
using Hardcodet.Wpf.TaskbarNotification;
using Kombajn_Shortcut.Helpers;
using Kombajn_Shortcut.Models;
using Kombajn_Shortcut.Views;
using KombajnDoPracy.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Kombajn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon NotifyIcon { get; set; }
        private GlobalHookBase GlobalHook { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            NotifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            Screener screener = new Screener(); //to make screen

            List<ShortcutModel> Shurtcuts = new List<ShortcutModel>
            {
                new ShortcutModel()
                {
                    Name = "Kombajn do pracy",
                    Describes = "Skrótowe wejście w potrzebne foldery",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    KeyCode = "1",
                    actionOnClick = () =>
                    {
                         Process.Start(AppsPath.WorkBoardPath);
                    }
                },

                new ShortcutModel()
                {
                    Name = "Screener",
                    Describes = "Robi screena aktualnego monitora i wrzuca go do folderu zbiorczego z innymi screenami",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    IsMouseDoubleMiddleClick = true,
                    KeyCode = "2",
                    actionOnClick = () =>
                    {
                        try
                        {
                            string screenPath = screener.TakeScreenshot();
                            var screenAnimation = new ScreenAnimationWindow(screenPath);
                            screenAnimation.Left = SystemParameters.VirtualScreenWidth - 250;
                            screenAnimation.Top = 0;

                            screenAnimation.Show();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                            LogError(ex.ToString());
                        }
                    }
                },

                new ShortcutModel()
                {
                    Name = "Screener Alt Modificator",
                    Describes = "Robi screena aktualnego monitora i wrzuca go do folderu zbiorczego z innymi screenami",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Alt,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    KeyCode = "2",
                    actionOnClick = () =>
                    {
                        try
                        {
                            string screenPath = screener.TakeScreenshot();
                            var screenAnimation = new ScreenAnimationWindow(screenPath);
                            screenAnimation.Left = SystemParameters.VirtualScreenWidth - 250;
                            screenAnimation.Top = 0;

                            screenAnimation.Show();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                            LogError(ex.ToString());
                        }
                    }
                },

                new ShortcutModel()
                {
                    Name = "Linijka",
                    Describes = "Miarka pikselów na ekranie",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    KeyCode = "L",
                    actionOnClick = () =>
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = AppsPath.LinijkaPath;
                        process.Start();
                        IntPtr handle = process.Handle;
                        WinApiHelper.SetForegroundWindow(handle);
                    }
                },

                new ShortcutModel()
                {
                    Name = "Color Picker",
                    Describes = "Pokazuje kolor z hexa",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    KeyCode = "D",
                    actionOnClick = () =>
                    {
                        Process.Start(AppsPath.GetColorPath);
                    }
                },

                new ShortcutModel()
                {
                    Name = "Open Daily text file",
                    Describes = "Otwiera plik tekstowy z folderu Notatki na dany dzień",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    KeyCode = "3",
                    actionOnClick = () =>
                    {
                        string dailyTextFile = ButtonPathGenerator.GetDailyFileFullPath(".txt");

                        var processStartInfo = new ProcessStartInfo() { FileName = dailyTextFile, UseShellExecute = true };
                        var process = Process.Start(processStartInfo);

                        IntPtr handle = process.Handle;
                        WinApiHelper.SetForegroundWindow(handle);
                    }
                },

                new ShortcutModel()
                {
                    Name = "Open All Dane -> Ważne Notsy",
                    Describes = "Otwiera plik tekstowy z ważnym tekstem",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    KeyCode = "4",
                    actionOnClick = () =>
                    {
                        string dailyTextFile = ButtonPathGenerator.GetAllDataNoteFullPath();

                        var processStartInfo = new ProcessStartInfo() { FileName = dailyTextFile, UseShellExecute = true };
                        var process = Process.Start(processStartInfo);
                        IntPtr handle = process.Handle;
                        WinApiHelper.SetForegroundWindow(handle);
                    }
                },
                new ShortcutModel()
                {
                    Name = "Quit PC",
                    Describes = "Wyłącza PC",
                    AllModifs = true,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.Alt,
                    KeyCode = "Q",
                    actionOnClick = () =>
                    {
                        //MessageBox.Show("shutdown");
                        Process.Start("shutdown", "/s /t 0");
                    }
                }
            };
            //Set global hook
            GlobalHook = new GlobalHookBase(Shurtcuts);

            //assign list to notifyIcon
            if (NotifyIcon != null)
                NotifyIcon.DataContext = new NotifyIconViewModel(Shurtcuts);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            LogError(e.Exception.ToString());
            e.Handled = true;
        }

        private void LogError(string errorMsg)
        {
            string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationFolder = "KombajnDeluxe Data";
            string fullPath = Path.Combine(myDocumentPath, applicationFolder);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            var ErrorLogPath = Path.Combine(fullPath, "ErrorLogsKombajnShortcut.txt");

            string errorContent = $">>>>>{DateTime.Now}{Environment.NewLine}{errorMsg}{Environment.NewLine}-----------------{Environment.NewLine}{Environment.NewLine}";

            File.AppendAllText(ErrorLogPath, errorContent);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NotifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            GlobalHook.Dispose();
            base.OnExit(e);
        }
    }
}