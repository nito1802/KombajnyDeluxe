using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kombajn_Shortcut.Helpers
{
    public static class WinApiHelper
    {
        private const string DLL_NAME = "user32.dll";

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            int left, top, right, bottom;

            public Rectangle ToRectangle()
            {
                return new Rectangle(left, top, right - left, bottom - top);
            }
        }

        [DllImport(DLL_NAME)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);


        [DllImport(DLL_NAME)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport(DLL_NAME)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(DLL_NAME, SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, String className, String windowTitle);

        [DllImport(DLL_NAME)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport(DLL_NAME)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public static Rectangle GetWindowRect(IntPtr hWnd)
        {
            var nativeRect = new RECT();
            GetWindowRect(hWnd, out nativeRect);
            return nativeRect.ToRectangle();
        }
    }
}
