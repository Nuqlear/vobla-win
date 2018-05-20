using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace vobla
{
    public abstract class CaptureScreen
    {
        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);
    
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject", SetLastError = true)]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);

        public static Image CaptureRectangle(Rectangle rectangle)
        {
            var handle = GetDesktopWindow();
            IntPtr hdcSrc = GetWindowDC(handle);
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, rectangle.Width, rectangle.Height);
            IntPtr hOld = SelectObject(hdcDest, hBitmap);
            BitBlt(hdcDest, 0, 0, rectangle.Width - 1, rectangle.Height - 1, hdcSrc, rectangle.X + 1, rectangle.Y + 1, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            SelectObject(hdcDest, hOld);
            DeleteDC(hdcDest);
            ReleaseDC(handle, hdcSrc);
            Image image = Image.FromHbitmap(hBitmap);
            DeleteDC(hBitmap);
            return image;
        }

        public static Image CaptureFullscreen()
        {
            Rectangle rect = SystemInformation.VirtualScreen;
            return CaptureRectangle(rect);
        }
    }
}
