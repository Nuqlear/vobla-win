using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace vobla
{
    public delegate void AreaSelected(Rectangle rect);

    class AreaSelector: Form
    {
        #region WinApiConstants
        private const int WM_SETCURSOR = 0x0020;
        private const int IDC_CROSS = 32515;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TOOLWINDOW = 0x80;
        #endregion

        private System.Drawing.Point SelectionStart;
        private System.Drawing.Point SelectionEnd;
        private Rectangle area;
        public event AreaSelected areaSelectedEvent;

        public AreaSelector()
        {
            this.area = default(Rectangle);

            this.DoubleBuffered = true;

            /* making transparent window */
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Cyan;
            this.TransparencyKey = Color.Cyan;

            Paint += new PaintEventHandler(this.PaintSelection);
            MouseDown += new MouseEventHandler(this.MouseDownEvent);
            MouseMove += new MouseEventHandler(this.MouseMoveEvent);
            MouseUp += new MouseEventHandler(this.MouseUpEvent);
            KeyDown += new KeyEventHandler(this.KeyEvent);
        }

        void PaintSelection(object sender, PaintEventArgs e)
        {
            if (this.SelectionStart.IsEmpty)
            {
                return;
            }
            var pt = Cursor.Position;
            var location = new System.Drawing.Point(
                Math.Min(this.SelectionStart.X, this.SelectionEnd.X),
                Math.Min(this.SelectionStart.Y, this.SelectionEnd.Y)
            );
            this.SelectionEnd = pt;
            var destination = new System.Drawing.Point(
                Math.Max(this.SelectionStart.X, this.SelectionEnd.X),
                Math.Max(this.SelectionStart.Y, this.SelectionEnd.Y)
            );
            var size = new System.Drawing.Size(destination.X - location.X, destination.Y - location.Y);
            this.area = new Rectangle(location, size);
            e.Graphics.DrawRectangle(Pens.Purple, this.area);
        }

        private void CancelSelection()
        {
            this.area = default(Rectangle);
            this.EndSelection();
        }

        private void EndSelection()
        {
            this.areaSelectedEvent(this.area);
            this.Dispose();
        }

        private void MouseDownEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.SelectionStart = this.SelectionEnd = Cursor.Position;
                this.Refresh();
            }
        }

        private void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            this.Refresh();
        }

        private void MouseUpEvent(object sender, MouseEventArgs e)
        {
            this.EndSelection();
        }

        private void KeyEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.CancelSelection();
            }
        }

        /* 
        WS_EX_LAYERED window style allows window to be transparent.
        If the layered window has the WS_EX_TOOLWINDOW extended window style,
        the layered window will not be showed in the alt-tab menu.
        */
        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                Params.ExStyle |= (WS_EX_LAYERED | WS_EX_TOOLWINDOW);
                return Params;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetCursor(IntPtr hCursor);
        
        protected override void WndProc(ref Message m)
        {
            var handled = false;
            if (m.Msg == WM_SETCURSOR)
            {
                SetCursor(LoadCursor(IntPtr.Zero, IDC_CROSS));
                handled = true;
            }
            if (handled) DefWndProc(ref m); else base.WndProc(ref m);
        }
    
    }
}
