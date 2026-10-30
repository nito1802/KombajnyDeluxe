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
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Changed(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
