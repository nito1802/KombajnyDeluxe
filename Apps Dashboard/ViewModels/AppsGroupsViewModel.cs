using Apps_Dashboard.Helpers;
using Apps_Dashboard.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Apps_Dashboard.ViewModels
{
    public class AppsGroupsViewModel
    {
        private static readonly string UserConfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Apps Dashboard", "AppsInputData.json");
        public ObservableCollection<AppsGroupModel> AppsGroups { get; set; }
        public ICommand LaunchAppCommand { get; }
        public static Action? MinimizeApplication { get; set; }
        public AppsGroupsViewModel()
        {
            var config = File.Exists(UserConfig) ? UserConfig : Path.Combine(AppContext.BaseDirectory, "AppsInputData.json");
            AppsGroups = JsonConvert.DeserializeObject<ObservableCollection<AppsGroupModel>>(File.ReadAllText(config)) ?? [];
            LaunchAppCommand = new RelayCommand(LaunchApp);
        }
        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UserConfig)!);
            var temporary = UserConfig + ".tmp";
            File.WriteAllText(temporary, JsonConvert.SerializeObject(AppsGroups, Formatting.Indented));
            File.Move(temporary, UserConfig, true);
        }
        private void LaunchApp(object path)
        {
            if (path is not string appPath) return;
            try
            {
                Process.Start(new ProcessStartInfo(appPath) { UseShellExecute = true });
                MinimizeApplication?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się uruchomić aplikacji: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
