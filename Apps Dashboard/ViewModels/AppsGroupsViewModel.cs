using Apps_Dashboard.Models;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Apps_Dashboard.ViewModels
{
    public class AppsGroupsViewModel
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        private class Win32
        {
            public const uint SHGFI_ICON = 0x000000100;
            public const uint SHGFI_LARGEICON = 0x000000000;
            public const uint SHGFI_SMALLICON = 0x000000001;
            public const uint SHGFI_EXTRALARGEICON = 0x000000002;
            public const uint SHGFI_SYSICONINDEX = 0x00004000;
            public const uint SHGFI_ICONLOCATION = 0x000001000;

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);
        }

        public ObservableCollection<AppsGroupModel> AppsGroups { get; set; }

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
        }

        private ObservableCollection<SingleAppModel> GenerateSampleApps()
        {
            var sampleApps = new ObservableCollection<SingleAppModel>();
            for (int i = 0; i < 10; i++)
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
    }
}