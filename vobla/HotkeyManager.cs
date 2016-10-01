using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;

// copy-paste

namespace vobla
{
    public class HotkeyManager
    {
        public delegate void HotKeyPressedEventHandler(object sender, EventArgs e);
        public static event HotKeyPressedEventHandler hotKeyPressedEvent;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        public static void AddGlobalKeyHook(Window window, uint modifierKeyID, uint vk)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            var hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.AddHook(HwndHook);
            RegisterHotKey(helper.Handle, WinApiConstants.HOTKEY_ID, modifierKeyID, vk);
        }

        public static void AddGlobalKeyHook(Form form, uint modifierKeyID, uint vk)
        {
            RegisterHotKey(form.Handle, WinApiConstants.HOTKEY_ID, modifierKeyID, vk);
        }

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        public static void RemoveGlobalKeyHook(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            HwndSource hwndSource = HwndSource.FromHwnd(helper.Handle);
            hwndSource.RemoveHook(HwndHook);
            UnregisterHotKey(helper.Handle, WinApiConstants.HOTKEY_ID);
        }

        public static void RemoveGlobalKeyHook(Form form)
        {
            UnregisterHotKey(form.Handle, WinApiConstants.HOTKEY_ID);
        }

        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WinApiConstants.WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case WinApiConstants.HOTKEY_ID:
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
