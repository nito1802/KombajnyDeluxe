using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Apps_Dashboard.Helpers
{
    internal static class ShellAppIcon
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ImageSize { public int Width; public int Height; }

        [ComImport, Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            int GetImage(ImageSize size, uint flags, out IntPtr bitmap);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName(string name, IntPtr context, ref Guid iid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory? factory);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr bitmap);

        public static BitmapSource? Load(string path)
        {
            IShellItemImageFactory? factory = null;
            IntPtr bitmap = IntPtr.Zero;
            try
            {
                // Resolve the AppsFolder namespace item, rather than treating it as a file.
                var iid = typeof(IShellItemImageFactory).GUID;
                if (path.StartsWith(@"shell:AppsFolder\", StringComparison.OrdinalIgnoreCase))
                    path = @"::{4234d49b-0245-4df3-b780-3893943456e1}\" + path.Substring(@"shell:AppsFolder\".Length);
                if (SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid, out factory) < 0 || factory == null)
                    return null;
                const uint AppImage = 0; // AppsFolder supplies the app logo as its thumbnail.
                if (factory.GetImage(new ImageSize { Width = 64, Height = 64 }, AppImage, out bitmap) < 0 || bitmap == IntPtr.Zero)
                    return null;
                var image = Imaging.CreateBitmapSourceFromHBitmap(bitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                image.Freeze();
                return image;
            }
            catch (COMException) { return null; }
            finally
            {
                if (bitmap != IntPtr.Zero) DeleteObject(bitmap);
                if (factory != null) Marshal.ReleaseComObject(factory);
            }
        }
    }
}