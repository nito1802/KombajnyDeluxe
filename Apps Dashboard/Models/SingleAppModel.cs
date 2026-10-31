using Apps_Dashboard.Helpers;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Apps_Dashboard.Models
{
    public class SingleAppModel : INotifyPropertyChanged
    {
        private string path = string.Empty;
        private string displayName = string.Empty;
        public int? LayoutColumn { get; set; }
        public int? LayoutRow { get; set; }
        public string Path
        {
            get => path;
            set { path = value; Icon = GetIconFromFile(value); Changed(nameof(Path)); Changed(nameof(Icon)); }
        }
        [JsonIgnore]
        public BitmapSource? Icon { get; private set; }
        public string DisplayName
        {
            get => displayName;
            set { displayName = value; Changed(nameof(DisplayName)); }
        }
        public List<DateTime> Clicks { get; set; } = [];
        public event PropertyChangedEventHandler? PropertyChanged;
        private void Changed(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        private static BitmapSource? GetIconFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            filePath = Environment.ExpandEnvironmentVariables(filePath.Trim().Trim('"').Trim());
            if (filePath.StartsWith("shell:AppsFolder\\", StringComparison.OrdinalIgnoreCase))
                return PackagedAppIcon.Load(filePath) ?? ShellAppIcon.Load(filePath);
            // Read executable resources directly, bypassing stale Explorer icon cache entries.
            if (System.IO.Path.GetExtension(filePath).Equals(".exe", StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(filePath))
            {
                Win32.ExtractIconEx(filePath, 0, out var large, out var small, 1);
                try
                {
                    var handle = large != IntPtr.Zero ? large : small;
                    if (handle != IntPtr.Zero)
                    {
                        var image = Imaging.CreateBitmapSourceFromHIcon(handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        image.Freeze();
                        return image;
                    }
                }
                finally
                {
                    if (large != IntPtr.Zero) Win32.DestroyIcon(large);
                    if (small != IntPtr.Zero && small != large) Win32.DestroyIcon(small);
                }
            }
            var info = new SHFILEINFO();
            Win32.SHGetFileInfo(filePath, 0, ref info, (uint)Marshal.SizeOf(info), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
            if (info.hIcon == IntPtr.Zero) return null;
            try { return Imaging.CreateBitmapSourceFromHIcon(info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); }
            finally { Win32.DestroyIcon(info.hIcon); }
        }
    }
}

