using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;


namespace vobla
{
    public delegate void AreaSelected(Rectangle rect);

    class AreaSelector : Form
    {
        private System.Drawing.Point _selectionStart;
        private System.Drawing.Point _selectionEnd;
        private Rectangle _area;
        private readonly IKeyboardMouseEvents _globalHook;
        private readonly Dictionary<int, IntPtr> _cursorsBackup;

        public event AreaSelected AreaSelectedEvent;

        public AreaSelector()
        {
            this._cursorsBackup = new Dictionary<int, IntPtr>();
            foreach (var cursorId in WinApiConstants.Cursors())
            {
                _cursorsBackup[cursorId] = CopyIcon(LoadCursor(IntPtr.Zero, cursorId));
                SetSystemCursor(CopyIcon(LoadCursor(IntPtr.Zero, WinApiConstants.IDC_CROSS)), cursorId);
            }

            this._area = default(Rectangle);
            this.DoubleBuffered = true;
            /* making transparent window */
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Cyan;
            this.TransparencyKey = Color.Cyan;
            this._globalHook = Hook.GlobalEvents();
            this._globalHook.MouseDownExt += this.MouseDownEvent;
            this._globalHook.MouseUpExt += this.MouseUpEvent;
            this._globalHook.MouseMoveExt += this.MouseMoveEvent;
            this._globalHook.MouseMoveExt -= null;
            Paint += new PaintEventHandler(this.PaintEvent);

            HotkeyManager.AddGlobalKeyHook(this, 0x0000, WinApiConstants.VK_ESCAPE);
        }

        void PaintEvent(object sender, PaintEventArgs e)
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

        private void StartSelection()
        {
            this._selectionStart = this._selectionEnd = Cursor.Position;
            this.Refresh();
        }

        private void CancelSelection()
        {
            this._area = default(Rectangle);
            this.EndSelection();
        }

        private void EndSelection()
        {
            foreach (KeyValuePair<int, IntPtr> cursor in this._cursorsBackup)
            {
                SetSystemCursor(cursor.Value, cursor.Key);
            }
            this.AreaSelectedEvent?.Invoke(this._area);
            HotkeyManager.RemoveGlobalKeyHook(this);
            this.Dispose();
        }

        private void MouseDownEvent(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.StartSelection();
            }
//            e.Handled = true;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CopyIcon(IntPtr hIcon);
        //                SetSystemCursor(CopyIcon(Cursors.Cross.Handle), 32512);

        private void MouseMoveEvent(object sender, MouseEventExtArgs e)
        {
            this.Refresh();
        }

        private void MouseUpEvent(object sender, MouseEventExtArgs e)
        {
            this.EndSelection();
            e.Handled = true;
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

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hCursor, int lpCursorName);

        protected override void WndProc(ref Message m)
        {
            var handled = false;
            //            if (m.Msg == WinApiConstants.WM_SETCURSOR)
            //            {
            //                SetCursor(LoadCursor(IntPtr.Zero, WinApiConstants.IDC_CROSS));
            //                handled = true;
            //            }
            if (m.Msg == WinApiConstants.WM_HOTKEY)
            {
                this.CancelSelection();
            }
            if (handled) DefWndProc(ref m); else base.WndProc(ref m);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AreaSelector));
            this.SuspendLayout();
            // 
            // AreaSelector
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AreaSelector";
            this.ResumeLayout(false);

        }

        protected override void Dispose(bool disposing)
        {
            this._globalHook.Dispose();
            base.Dispose(disposing);
        }
    }
}
