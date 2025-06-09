using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Flow.Launcher.Plugin.YoutubeMusicSearch
{
    class ProcessUtils {

        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32")]
        public static extern int ShowWindow(int hwnd, int nCmdShow);

        public const UInt32 WM_CLOSE = 0x0010;

        public const int SW_MINIMIZE = 6;


        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<IntPtr, string> GetOpenWindows() {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            EnumWindows(delegate (IntPtr hWnd, int lParam) {
                if(hWnd == shellWindow) return true;
                if(!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if(length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }

        public static void CloseWindowWhoContains(string titlePart) {
            foreach(KeyValuePair<IntPtr, string> window in ProcessUtils.GetOpenWindows()) {
                IntPtr handle = window.Key;
                string title = window.Value;

                if(title.Contains(titlePart)) {
                    ProcessUtils.SendMessage(handle, ProcessUtils.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        public static void MinimizeWindow(string titlePart) {
            foreach(KeyValuePair<IntPtr, string> window in ProcessUtils.GetOpenWindows()) {
                IntPtr handle = window.Key;
                string title = window.Value;

                if(title.Contains(titlePart)) {
                    ShowWindow(handle.ToInt32(), SW_MINIMIZE);
                }
            }
        }
    }
}
