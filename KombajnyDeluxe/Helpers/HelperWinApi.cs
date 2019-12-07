using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KombajnDoPracy
{
    class HelperWinApi
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    }
}
