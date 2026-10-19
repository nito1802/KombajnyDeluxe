using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KombajnDoPracy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Exception: " + e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);

            string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string applicationFolder = "KombajnDeluxe Data";

            string fullPath = Path.Combine(myDocumentPath, applicationFolder);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            var ErrorLogPath = Path.Combine(fullPath, "ErrorLogsKombajnDoPracy.txt");

            string errorContent = $">>>>>{DateTime.Now}{Environment.NewLine}{e.Exception.ToString()}{Environment.NewLine}-----------------{Environment.NewLine}{Environment.NewLine}";

            File.AppendAllText(ErrorLogPath, errorContent);

            e.Handled = true;
        }
    }
}
