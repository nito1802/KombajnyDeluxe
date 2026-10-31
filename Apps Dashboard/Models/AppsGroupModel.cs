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
        private int maxColumns;
        public int MaxColumns { get => maxColumns; set { maxColumns = Math.Max(0, value); Changed(nameof(MaxColumns)); } }
        public void NormalizeColumns()
        {
            if (MaxColumns == 0) return;
            var used = new HashSet<(int, int)>();
            foreach (var app in Apps.OrderBy(a => a.LayoutRow ?? 0).ThenBy(a => a.LayoutColumn ?? 0))
            {
                int column = Math.Max(0, app.LayoutColumn ?? 0), row = Math.Max(0, app.LayoutRow ?? 0);
                row += column / MaxColumns; column %= MaxColumns;
                while (!used.Add((column, row))) { column++; if (column == MaxColumns) { column = 0; row++; } }
                app.LayoutColumn = column; app.LayoutRow = row;
            }
        }
        public double? LayoutX { get; set; }
        public double? LayoutY { get; set; }
        private string? backgroundPath;
        public string? BackgroundPath { get => backgroundPath; set { backgroundPath = value?.Trim().Trim('"').Trim(); texture = null; Changed(nameof(BackgroundPath)); Changed(nameof(Texture)); } }
        private System.Windows.Media.Imaging.BitmapImage? texture;
        [JsonIgnore] public ImageSource? Texture
        {
            get
            {
                if (texture != null) return texture;
                if (string.IsNullOrWhiteSpace(backgroundPath)) return null;
                try
                {
                    using var stream = System.IO.File.OpenRead(backgroundPath);
                    var image = new System.Windows.Media.Imaging.BitmapImage(); image.BeginInit();
                    image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    image.DecodePixelWidth = 1024; image.StreamSource = stream; image.EndInit(); image.Freeze(); texture = image;
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
            if (!Apps.Contains(app)) return;
            if (destination == this && app.LayoutColumn == column && (app.LayoutRow ?? 0) == row) return;
            var target = destination.Apps.FirstOrDefault(a => a != app && a.LayoutColumn == column && (a.LayoutRow ?? 0) == row);
            // Keep insertion anchored to the hovered tile when closing the source gap.
            RemoveAppAndCloseGap(app);
            if (target != null) { column = target.LayoutColumn ?? 0; row = target.LayoutRow ?? 0; }
            var occupied = destination.Apps.ToDictionary(a => (a.LayoutColumn ?? 0, a.LayoutRow ?? 0));
            int nextColumn = column, nextRow = row;
            // Push only the occupied chain; wrap at the group's column limit.
            while (occupied.TryGetValue((nextColumn, nextRow), out var following))
            {
                nextColumn++;
                if (destination.MaxColumns > 0 && nextColumn >= destination.MaxColumns) { nextColumn = 0; nextRow++; }
                following.LayoutColumn = nextColumn; following.LayoutRow = nextRow;
            }
            app.LayoutColumn = column; app.LayoutRow = row;
            destination.Apps.Add(app);
            destination.NormalizeColumns();
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Changed(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
