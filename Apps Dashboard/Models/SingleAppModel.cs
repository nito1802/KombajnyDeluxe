using System.Windows.Media.Imaging;

namespace Apps_Dashboard.Models
{
    public class SingleAppModel
    {
        public BitmapSource Icon { get; set; }
        public string Path { get; set; }
        public string DisplayName { get; set; }
        public List<DateTime> Clicks { get; set; } = [];
    }
}