using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Kombajn
{
    internal class Screener
    {
        public string TakeScreenshot(string directoryName)
        {
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            //double screenWidth = SystemParameters.VirtualScreenWidth;
            //double screenHeight = SystemParameters.VirtualScreenHeight;
            double screenWidth = Screen.PrimaryScreen.Bounds.Width;
            double screenHeight = Screen.PrimaryScreen.Bounds.Height;

            string screenerPath = GetScreenerPath(directoryName);
            String filename = $"{Directory.GetFiles(screenerPath).Length + 1} - {DateTime.Now.ToString("HH_mm_ss")}.png";
            string fullPath = System.IO.Path.Combine(screenerPath, filename);

            using System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)screenWidth, (int)screenHeight);
            using System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);

            bmp.Save(fullPath);
            return fullPath;
        }

        private string GetScreenerPath(string directoryName)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string mojeDanePath = KombajnCommon.MojeDanePath;

            if (!Directory.Exists(mojeDanePath)) Directory.CreateDirectory(mojeDanePath);

            var polishFormat = new CultureInfo("pl-PL");
            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.ToString("MMMM", polishFormat);
            string currentDayFormat = DateTime.Now.ToString("dd_MM_yyyy");
            string dataType = directoryName;

            var screenerPath = System.IO.Path.Combine(mojeDanePath, currentYear, currentMonth, currentDayFormat, dataType);

            if (!Directory.Exists(screenerPath))
            {
                Directory.CreateDirectory(screenerPath);
            }

            return screenerPath;
        }
    }
}