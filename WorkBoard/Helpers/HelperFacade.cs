using System.Diagnostics;

namespace KombajnDoPracy
{
    class ProcessHelper
    {
        public static void OpenOrRestoreWindow(string selectedPath)
        {
            //string selectedPathEncoded = HttpUtility.UrlPathEncode(selectedPath).Replace("\\", "/");

            //SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();

            //foreach (SHDocVw.InternetExplorer window in shellWindows)
            //{
            //    string filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();

            //    if (filename.Equals("explorer"))
            //    {
            //        string path = window.LocationURL;

            //        if (!string.IsNullOrEmpty(path))
            //        {
            //            path = path.Replace("file:///", "");

            //            if (path == selectedPathEncoded)
            //            {
            //                HelperWinApi.ShowWindow((IntPtr)window.HWND, 9);
            //                return;
            //            }
            //        }
            //    }
            //}

            var processStartInfo = new ProcessStartInfo() { FileName = selectedPath, UseShellExecute = true };
            Process.Start(processStartInfo);
        }
    }
}
