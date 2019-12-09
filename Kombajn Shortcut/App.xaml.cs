using Hardcodet.Wpf.TaskbarNotification;
using Kombajn_Shortcut.Helpers;
using Kombajn_Shortcut.Models;
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
                        string kombajnDoPracyPath = "KombajnDoPracy.exe";
                        Process.Start(kombajnDoPracyPath);
                    }
                },

                new ShortcutModel()
                {
                    Name = "Screener",
                    Describes = "Robi screena aktualnego monitora i wrzuca go do folderu zbiorczego z innymi screenami",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    KeyCode = "2",
                    actionOnClick = () =>
                    {
                        try
                        {
                            screener.TakeScreenshot();
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
                    Name = "Narzędzie wycinania",
                    Describes = "Wycina określony obszar",
                    AllModifs = false,
                    SelectedModificator = ModificatorNumeration.Ctrl,
                    AlternateSelectedModificator = ModificatorNumeration.None,
                    KeyCode = "E",
                    actionOnClick = () =>
                    {
                        string snippingToolProcess = string.Empty;

                        if (!Environment.Is64BitProcess)
                            snippingToolProcess = @"C:\Windows\sysnative\SnippingTool.exe";
                        else
                            snippingToolProcess = @"C:\WINDOWS\system32\SnippingTool.exe";

                        Process process = new Process();
                        process.StartInfo.FileName = snippingToolProcess;
                        process.Start();
                        IntPtr handle = process.Handle;
                        WinApiHelper.SetForegroundWindow(handle);
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
                        string rulerProcessPath = @"C:\Program Files (x86)\Best Apps Ever\Linijka\Linijka.exe";

                        Process process = new Process();
                        process.StartInfo.FileName = rulerProcessPath;
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
                    KeyCode = "T",
                    actionOnClick = () =>
                    {
                        string snippingCzasowyWylacznik = @"C:\Users\dante\Source\Repos\GetColor2\GetColor\bin\Release\GetColor.exe";
                        Process.Start(snippingCzasowyWylacznik);
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