using Apps_Dashboard.Helpers;
using Apps_Dashboard.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Apps_Dashboard.ViewModels
{
    public class AppsGroupsViewModel
    {
        public ObservableCollection<AppsGroupModel> AppsGroups { get; set; }
        public ICommand LaunchAppCommand { get; }
        public static Action MinimizeApplication { get; set; } = null;

        public AppsGroupsViewModel()
        {
            var allText = File.ReadAllText("AppsInputData.json");
            AppsGroups = JsonConvert.DeserializeObject<ObservableCollection<AppsGroupModel>>(allText);

            var serialized = JsonConvert.SerializeObject(AppsGroups, Formatting.Indented);

            LaunchAppCommand = new RelayCommand(LaunchApp);
        }

        private BitmapSource GetIconFromFile(string filePath)
        {
            var shinfo = new SHFILEINFO();
            Win32.SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | Win32.SHGFI_EXTRALARGEICON);
            return Imaging.CreateBitmapSourceFromHIcon(
                shinfo.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private void LaunchApp(object path)
        {
            if (path is string appPath)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(appPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nie udało się uruchomić aplikacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                MinimizeApplication();
            }
        }
    }
}