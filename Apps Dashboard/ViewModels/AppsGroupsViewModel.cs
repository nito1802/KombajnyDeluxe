using Apps_Dashboard.Helpers;
using Apps_Dashboard.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
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

        public AppsGroupsViewModel()
        {
            AppsGroups = new ObservableCollection<AppsGroupModel>
            {
                new AppsGroupModel
                {
                    GroupName = "MOJE",
                    Apps = GenerateSampleApps()
                },
                new AppsGroupModel
                {
                    GroupName = "PROGRAMOWANIE",
                    Apps = GenerateSampleApps()
                },
                new AppsGroupModel
                {
                    GroupName = "ART",
                    Apps = GenerateSampleApps()
                },
                new AppsGroupModel
                {
                    GroupName = "INNE",
                    Apps = GenerateSampleApps()
                }
            };

            var serialized = JsonConvert.SerializeObject(AppsGroups, Formatting.Indented);

            LaunchAppCommand = new RelayCommand(LaunchApp);
        }

        private ObservableCollection<SingleAppModel> GenerateSampleApps()
        {
            var sampleApps = new ObservableCollection<SingleAppModel>();
            for (int i = 0; i < 2; i++)
            {
                sampleApps.Add(new SingleAppModel
                {
                    DisplayName = $"FL Studio {i + 1}",
                    Path = @"C:\Program Files\Image-Line\FL Studio 21\FL64.exe",
                    Icon = GetIconFromFile(@"C:\Program Files\Image-Line\FL Studio 21\FL64.exe")
                });
            }
            return sampleApps;
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
            }
        }
    }
}