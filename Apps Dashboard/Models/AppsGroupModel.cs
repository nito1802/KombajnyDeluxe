using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Apps_Dashboard.Models
{
    public class AppsGroupModel : INotifyPropertyChanged
    {
        private string name = "Nowa grupa";
        private string color = "#C99045";
        public double? LayoutX { get; set; }
        public double? LayoutY { get; set; }
        private string? backgroundPath;
        public string? BackgroundPath { get => backgroundPath; set { backgroundPath = value; texture = null; Changed(nameof(BackgroundPath)); Changed(nameof(Texture)); } }
        private System.Windows.Media.Imaging.BitmapImage? texture;
        [JsonIgnore] public ImageSource? Texture
        {
            get
            {
                if (texture != null) return texture;
                if (string.IsNullOrWhiteSpace(backgroundPath)) return null;
                try
                {
                    var image = new System.Windows.Media.Imaging.BitmapImage(); image.BeginInit();
                    image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = 1024; image.UriSource = new Uri(backgroundPath, UriKind.Absolute); image.EndInit(); image.Freeze(); texture = image;
                }
                catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or System.IO.FileFormatException) { }
                return texture;
            }
        }
        public string GroupName { get => name; set { name = value; Changed(nameof(GroupName)); } }
        public string Color { get => color; set { color = value; Changed(nameof(Color)); Changed(nameof(Accent)); Changed(nameof(Outline)); Changed(nameof(Tint)); } }
        [JsonIgnore] public Brush Accent => MakeBrush(255);
        [JsonIgnore] public Brush Outline => MakeBrush(130);
        [JsonIgnore] public Brush Tint => MakeBrush(15);
        private Brush MakeBrush(byte alpha)
        {
            System.Windows.Media.Color parsed;
            try { parsed = (System.Windows.Media.Color)ColorConverter.ConvertFromString(color); }
            catch { parsed = Colors.Goldenrod; }
            parsed.A = alpha; var brush = new SolidColorBrush(parsed); brush.Freeze(); return brush;
        }
        public ObservableCollection<SingleAppModel> Apps { get; set; } = [];
        public void RemoveAppAndCloseGap(SingleAppModel app)
        {
            if (!Apps.Contains(app)) return;
            if (app.LayoutColumn is int column)
                foreach (var following in Apps.Where(a => a != app && (a.LayoutRow ?? 0) == (app.LayoutRow ?? 0) && a.LayoutColumn > column))
                    following.LayoutColumn--;
            Apps.Remove(app);
        }
        public void TransferAppTo(AppsGroupModel destination, SingleAppModel app, int column, int row)
        {
            if (destination == this || !Apps.Contains(app)) return;
            RemoveAppAndCloseGap(app);
            if (destination.Apps.Any(a => (a.LayoutRow ?? 0) == row && a.LayoutColumn == column))
                foreach (var following in destination.Apps.Where(a => (a.LayoutRow ?? 0) == row && a.LayoutColumn >= column))
                    following.LayoutColumn++;
            app.LayoutColumn = column; app.LayoutRow = row;
            destination.Apps.Add(app);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Changed(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
