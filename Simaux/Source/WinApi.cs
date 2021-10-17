using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public delegate bool CallBack(IntPtr hwnd, int lParam);
namespace WindowsApi
{
    public static class WinApi
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        public static extern bool PtInRect([In] ref RECT lprc, POINT pt);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point p);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32")]
        public static extern int EnumWindows(CallBack x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("gdi32.dll")]
        public static extern int GetPixel(IntPtr hDC, int x, int y);
        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
        [DllImport("user32.dll")]
        public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);
        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("gdi32", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
        [DllImport("gdi32", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hDestDC, int X, int Y, int nWidth, int nHeight, IntPtr hSrcDC, int SrcX, int SrcY, RasterOperations Rop);
        [DllImport("gdi32", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")]
        public static extern bool CloseWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, UIntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint wMsg, UIntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);
        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);
        [DllImport("user32.dll")]
        
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);
        
        public static int SWP_NOMOVE = 0X2;
        public static int SWP_NOSIZE = 1;
        public static int SWP_NOZORDER = 0X4;

        public static int SWP_SHOWWINDOW = 0x0040;
        public static int SWP_HIDEWINDOW = 0x0080;

        public static int GWL_STYLE = -16;

        public static int WS_CHILD = 0x40000000; //child window
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        public static uint WM_SYSCOMMAND = 0x0112;
        public static uint WM_LBUTTONDOWN = 0x0201;
        public static uint WM_LBUTTONUP = 0x0202;

        public static UIntPtr SC_CLOSE = (UIntPtr)0xF;

        public static IntPtr HWND_TOP = (IntPtr)0;
        public static IntPtr HWND_BOTTOM = (IntPtr)1;
        public static IntPtr HWND_TOPMOST = (IntPtr)(int)-1;
        public static IntPtr HWND_NOTOPMOST = (IntPtr)(int)-2;

        public enum RasterOperations : int
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
            public bool Equals(RECT other)
            {
                return Left == other.Left &&
                       Top == other.Top &&
                       Right == other.Right &&
                       Bottom == other.Bottom;
            }
            public new string ToString()
            {
                return string.Format("Left:{0} Top:{1} Right:{2} Bottom:{3}", Left, Top, Right, Bottom);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }
        public static IntPtr GetWindowUnderCursor()
        {
            GetCursorPos(out POINT lpPoint);
            return WindowFromPoint(lpPoint);
        }
        public enum EMousePosType { Client, Screen }
        public static void MouseClick(Point pos, EMousePosType mousePosType, IntPtr wnd)
        {
            uint dwFlags = 0;
            if (mousePosType == EMousePosType.Client)
            {
                ClientToScreen(wnd, ref pos);

            }
            dwFlags = MOUSEEVENTF_ABSOLUTE;

            mouse_event(MOUSEEVENTF_DOWN | dwFlags, pos.X, pos.Y, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_UP | dwFlags, pos.Y, pos.Y, 0, 0);
        }
        public static void MouseDown(Point pos)
        {
            mouse_event(MOUSEEVENTF_DOWN | MOUSEEVENTF_ABSOLUTE, pos.X, pos.Y, 0, 0);
        }
        public static void MouseUp(Point pos)
        {
            mouse_event(MOUSEEVENTF_UP | MOUSEEVENTF_ABSOLUTE, pos.X, pos.Y, 0, 0);
        }
        public static void MouseDownRight(Point pos)
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_ABSOLUTE, pos.X, pos.Y, 0, 0);
        }
        public static void MouseUpRight(Point pos)
        {
            mouse_event(MOUSEEVENTF_RIGHTUP | MOUSEEVENTF_ABSOLUTE, pos.X, pos.Y, 0, 0);
        }
        public static void MouseMove(Point pos)
        {
            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, pos.X, pos.Y, 0, 0);
        }
        public static string GetClassName(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(255);
            GetClassName(hwnd, sb, 255);
            return sb.ToString();
        }
        public static string GetWindowText(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(255);
            GetWindowText(hwnd, sb, 255);
            return sb.ToString();
        }
        public static void EnableTitleBar(IntPtr hwnd, bool enable)
        {
            int style = WinApi.GetWindowLong(hwnd, WinApi.GWL_STYLE);
            if (enable)
                style |= WinApi.WS_CAPTION;
            else
                style &= ~WinApi.WS_CAPTION;
            WinApi.SetWindowLong(hwnd, WinApi.GWL_STYLE, style);
        }
        public static Bitmap GetScreenshot(IntPtr hwnd)
        {
            RECT rc;
            var res = WinApi.GetWindowRect(hwnd, out rc);
            Bitmap bmp = new Bitmap(rc.Right - rc.Left, rc.Bottom - rc.Top, PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = gfxBmp.GetHdc();
            bool succeeded = WinApi.PrintWindow(hwnd, hdcBitmap, 0);
            gfxBmp.ReleaseHdc(hdcBitmap);
            if (!succeeded)
                gfxBmp.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(Point.Empty, bmp.Size));
            gfxBmp.Dispose();
            return bmp;
        }
        public static Rectangle GetWindowRectangle(IntPtr hwnd)
        {
            GetWindowRect(hwnd, out RECT rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left + 0, rect.Bottom - rect.Top + 0);
        }
        public static IntPtr SendMessage(IntPtr wnd, WM msg, uint wParam, int lParam)
        {
            return SendMessage(wnd, (uint)msg, (UIntPtr)wParam, (IntPtr)lParam);
        }
        public static IntPtr PostMessage(IntPtr wnd, WM msg, uint wParam, int lParam)
        {
            return PostMessage(wnd, (uint)msg, (UIntPtr)wParam, (IntPtr)lParam);
        }
        public static void SetWindowTopMost(IntPtr wnd, int sleepTime = 0, bool set = true)
        {
            if (set)
                WinApi.SetWindowPos(wnd, WinApi.HWND_TOPMOST, 0, 0, 0, 0, WinApi.SWP_NOMOVE | WinApi.SWP_NOSIZE);
            else
                WinApi.SetWindowPos(wnd, WinApi.HWND_NOTOPMOST, 0, 0, 0, 0, WinApi.SWP_NOMOVE | WinApi.SWP_NOSIZE);
            if (sleepTime != 0)
                Thread.Sleep(sleepTime);
        }
        public static IntPtr SetWindowUnderCursorTopMost()
        {
            var hwnd = GetWindowUnderCursor();
            if (hwnd != IntPtr.Zero)
            {
                SetWindowTopMost(hwnd);
                return hwnd;
            }
            return IntPtr.Zero;
        }
        static uint MOUSEEVENTF_MOVE = 0x0001;
        static uint MOUSEEVENTF_DOWN = 0x0002;
        static uint MOUSEEVENTF_UP = 0x0004;
        static uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        static uint MOUSEEVENTF_RIGHTUP = 0x0010;
        static uint MOUSEEVENTF_ABSOLUTE = 0x8000;
    }
}