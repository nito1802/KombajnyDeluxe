using Apps_Dashboard.Helpers;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Apps_Dashboard.Models
{
    public class SingleAppModel
    {
        private string path;

        public string Path
        {
            get { return path; }
            set
            {
                Icon = GetIconFromFile(value);

                path = value;
            }
        }

        public BitmapSource Icon { get; set; }

        //public string Path { get; set; }
        public string DisplayName { get; set; }

        public List<DateTime> Clicks { get; set; } = [];

        public SingleAppModel()
        {
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
    }
}