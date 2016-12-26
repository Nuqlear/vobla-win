using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace vobla
{
    public delegate void AreaSelected(Rectangle rect);

    class AreaSelector: Form
    {
        private System.Drawing.Point _selectionStart;
        private System.Drawing.Point _selectionEnd;
        private Rectangle _area;
        public event AreaSelected AreaSelectedEvent;

        public AreaSelector()
        {
            this._area = default(Rectangle);

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

            HotkeyManager.AddGlobalKeyHook(this, 0x0000, WinApiConstants.VK_ESCAPE);
        }

        void PaintSelection(object sender, PaintEventArgs e)
        {
            if (this._selectionStart.IsEmpty)
            {
                return;
            }
            var pt = Cursor.Position;
            var location = new System.Drawing.Point(
                Math.Min(this._selectionStart.X, this._selectionEnd.X),
                Math.Min(this._selectionStart.Y, this._selectionEnd.Y)
            );
            this._selectionEnd = pt;
            var destination = new System.Drawing.Point(
                Math.Max(this._selectionStart.X, this._selectionEnd.X),
                Math.Max(this._selectionStart.Y, this._selectionEnd.Y)
            );
            var size = new System.Drawing.Size(destination.X - location.X, destination.Y - location.Y);
            this._area = new Rectangle(location, size);
            e.Graphics.DrawRectangle(Pens.Purple, this._area);
        }

        private void CancelSelection()
        {
            this._area = default(Rectangle);
            this.EndSelection();
        }

        private void EndSelection()
        {
            this.AreaSelectedEvent?.Invoke(this._area);
            HotkeyManager.RemoveGlobalKeyHook(this);
            this.Dispose();
        }

        private void MouseDownEvent(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this._selectionStart = this._selectionEnd = Cursor.Position;
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
                Params.ExStyle |= (WinApiConstants.WS_EX_LAYERED | WinApiConstants.WS_EX_TOOLWINDOW);
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
            if (m.Msg == WinApiConstants.WM_SETCURSOR)
            {
                SetCursor(LoadCursor(IntPtr.Zero, WinApiConstants.IDC_CROSS));
                handled = true;
            }
            else if (m.Msg == WinApiConstants.WM_HOTKEY)
            {
                this.CancelSelection();
            }
            if (handled) DefWndProc(ref m); else base.WndProc(ref m);
        }
    
    }
}
