using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

// copy-paste

namespace vobla
{
    public class HotkeyManager
    {
        private static HwndSource hwndSource = null;
        public delegate void HotKeyPressedEventHandler(object sender, EventArgs e);
        public static event HotKeyPressedEventHandler hotKeyPressedEvent;

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        public static void AddGlobalKeyHook(Window window, uint modifierKeyID, uint vk)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.AddHook(HotkeyManager.HwndHook);
            RegisterHotKey(helper.Handle, HOTKEY_ID, modifierKeyID, vk);
        }

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        public static void RemoveGlobalKeyHook(Window window)
        {
            hwndSource.RemoveHook(HotkeyManager.HwndHook);
            var helper = new WindowInteropHelper(window);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            hotKeyPressedEvent(null, new EventArgs());
                            handled = true;
                            break;
                    }
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
