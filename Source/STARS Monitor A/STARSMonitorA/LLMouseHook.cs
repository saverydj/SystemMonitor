using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace STARSMonitorA
{
    static class LLMouseHook
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_MOUSE_LL = 14;

        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static LowLevelProc _proc = HookCallback;
        public static IntPtr _hookID = IntPtr.Zero;
        private static List<int> mouseMessages = new List<int>(new int[]
        {
            //0x0201, //WM_LBUTTONDOWN 
            //0x0202, //WM_LBUTTONUP 
            //0x0204, //WM_RBUTTONDOWN 
            //0x0205, //WM_RBUTTONUP 
            //0x0207, //WM_MBUTTONDOWN
            //0x0208, //WM_MBUTTONUP
            //0x0200,  //WM_MOUSEMOVE (introduces too much lag)
            0x020A //WM_MOUSEWHEEL     
        });

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (int)wParam == 0x020A)
            {
                Form2._isActivity = true;
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelProc proc)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
        }

        public static void Hook()
        {
            _hookID = SetHook(_proc);
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(_hookID);
        }

        public static void UnHook(long specifiedHookId)
        {
            UnhookWindowsHookEx(new IntPtr(specifiedHookId));
        }

    }
}
