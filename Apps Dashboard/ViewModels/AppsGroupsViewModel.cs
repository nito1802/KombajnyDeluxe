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
        private readonly string configPath = ResolveConfigPath(AppContext.BaseDirectory);
        public static string ResolveConfigPath(string baseDirectory)
        {
            // Debug/Release builds share the editable JSON in the project directory.
            for (var directory = new DirectoryInfo(baseDirectory); directory != null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Apps Dashboard.csproj")))
                    return Path.Combine(directory.FullName, "AppsInputData.json");
            }
            // A standalone copy uses its adjacent JSON, never a separate AppData file.
            return Path.Combine(baseDirectory, "AppsInputData.json");
        }
        public ObservableCollection<AppsGroupModel> AppsGroups { get; set; }
        public ICommand LaunchAppCommand { get; }
        public static Action? MinimizeApplication { get; set; }
        public AppsGroupsViewModel()
        {
            AppsGroups = JsonConvert.DeserializeObject<ObservableCollection<AppsGroupModel>>(File.ReadAllText(configPath)) ?? [];
            LaunchAppCommand = new RelayCommand(LaunchApp);
        }
        public void Save()
        {
            var temporary = configPath + ".tmp";
            File.WriteAllText(temporary, JsonConvert.SerializeObject(AppsGroups, Formatting.Indented));
            File.Move(temporary, configPath, true);
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
