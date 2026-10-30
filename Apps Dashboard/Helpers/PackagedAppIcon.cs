using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Apps_Dashboard.Helpers
{
    internal static class PackagedAppIcon
    {
        public static BitmapSource? Load(string shellPath)
        {
            try
            {
                var parts = shellPath.Substring(@"shell:AppsFolder\".Length).Split('!', 2);
                if (parts.Length != 2) return null;
                int split = parts[0].LastIndexOf('_');
                if (split < 0) return null;
                string packageName = parts[0][..split], publisher = parts[0][(split + 1)..];
                using var packages = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages");
                if (packages == null) return null;
                foreach (var key in packages.GetSubKeyNames().Where(n => n.StartsWith(packageName + "_", StringComparison.OrdinalIgnoreCase) && n.EndsWith("_" + publisher, StringComparison.OrdinalIgnoreCase)))
                {
                    using var package = packages.OpenSubKey(key);
                    if (package?.GetValue("PackageRootFolder") is not string root) continue;
                    var manifest = XDocument.Load(Path.Combine(root, "AppxManifest.xml"));
                    var app = manifest.Descendants().FirstOrDefault(e => e.Name.LocalName == "Application" && (string?)e.Attribute("Id") == parts[1]);
                    var visual = app?.Elements().FirstOrDefault(e => e.Name.LocalName == "VisualElements");
                    var logo = (string?)visual?.Attribute("Square44x44Logo") ?? (string?)visual?.Attribute("Square150x150Logo");
                    if (logo == null) continue;
                    string basePath = Path.Combine(root, logo);
                    string? imagePath = File.Exists(basePath) ? basePath : Directory.EnumerateFiles(Path.GetDirectoryName(basePath)!, Path.GetFileNameWithoutExtension(basePath) + ".*" + Path.GetExtension(basePath))
                        .OrderByDescending(p => p.Contains("targetsize-64", StringComparison.OrdinalIgnoreCase))
                        .ThenByDescending(p => p.Contains("altform-unplated", StringComparison.OrdinalIgnoreCase))
                        .ThenByDescending(p => p.Contains("scale-200", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (imagePath == null) continue;
                    var image = new BitmapImage(); image.BeginInit(); image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(imagePath); image.EndInit(); image.Freeze(); return image;
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException or System.Xml.XmlException or ArgumentException or NotSupportedException) { }
            return null;
        }
    }
}