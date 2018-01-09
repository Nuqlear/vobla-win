namespace vobla
{
    public static class WinApiConstants
    {
        public const int WM_HOTKEY = 0x0312;
        public const int WM_SETCURSOR = 0x0020;
        public const int HOTKEY_ID = 9000;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_4 = 0x34;
        public const int MOD_CTRL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int IDC_CROSS = 32515;
        public const int IDC_APPSTARTING = 32650;
        public const int IDC_ARROW = 32512;
        public const int IDC_HAND = 32649;
        public const int IDC_HELP = 32651;
        public const int IDC_IBEAM = 32513;
        public const int IDC_NO = 32648;
        public const int IDC_SIZE = 32640;
        public const int IDC_SIZEALL = 32646;
        public const int IDC_SIZENESW = 32643;
        public const int IDC_SIZENS = 32645;
        public const int IDC_SIZENWSE = 32642;
        public const int IDC_SIZEWE = 32644;
        public const int IDC_UPARROW = 32516;
        public const int IDC_WAIT = 32514;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TOOLWINDOW = 0x80;

        public static int[] Cursors()
        {
            int[] cursors =
            {
                IDC_APPSTARTING,
                IDC_ARROW,
                IDC_HAND,
                IDC_HELP,
                IDC_IBEAM,
                IDC_NO,
                IDC_SIZE,
                IDC_SIZEALL,
                IDC_SIZENESW,
                IDC_SIZENS,
                IDC_SIZENWSE,
                IDC_SIZEWE,
                IDC_UPARROW,
                IDC_WAIT
            };
            return cursors;
        }
    }
}
