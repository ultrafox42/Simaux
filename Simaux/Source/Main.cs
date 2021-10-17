using DebugWindow;
using FastBitmap;
using Gma.System.MouseKeyHook;
using MsfsConnect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using System.Windows.Forms;
using WindowsApi;

namespace Simaux
{
    public partial class Main : ApplicationContext
    {
        class PopupEntry
        {
            public IntPtr WindowHandle { get; set; } = IntPtr.Zero;
            public EMsfsWindowType WindowType { get; set; } = EMsfsWindowType.Other;
            public EToolbarWindows ToolbarWnd { get; set; } = EToolbarWindows.None;
            public Rectangle Rectangle { get; set; } = Rectangle.Empty;
            public double[] Histogram { get; set; } = null;
            public int Index { get; set; } = -1;

            public bool WindowExists()
            {
                if (WindowHandle == IntPtr.Zero)
                    return false;
                var className = WinApi.GetClassName(WindowHandle);
                return (className == Common.GetAppText(EAppTextLiterals.MsFsPopOutClassName));
            }
            public bool IsOnPosition()
            {
                var rect = WinApi.GetWindowRectangle(WindowHandle);
                return rect.Equals(Rectangle);
            }
            public override string ToString()
            {
                return string.Format(Common.GetAppText(EAppTextLiterals.DebugPopupEntryToString), Index, WindowType, ToolbarWnd, IsOnPosition(), WindowExists());
            }
        }
        #region Inits/DeInits
        public Main()
        {            
            InitCreateSysTrayEntry();
            InitFsConnection();          
            Log.ClearDebug();
            LoadSettings();
            InitHooks(false);
            InitTimer();
        }
        private void InitHooks(bool mouse)
        {
            m_GlobalHook = Hook.GlobalEvents();
            if (mouse)
            {
                m_GlobalHook.MouseDownExt += OnGlobalHookMouseDownExt;
                m_GlobalHook.MouseMoveExt += OnGlobalHookMouseMoveExt;
                m_GlobalHook.MouseUpExt += OnGlobalHookMouseUpExt;
            }
            m_GlobalHook.KeyPress += OnGlobalHookKeyPress;
            m_GlobalHook.KeyDown += OnGlobalHookKeyDown;
            m_GlobalHook.KeyUp += OnGlobalHookKeyUp;
            m_GlobalKeyDict = new Dictionary<Keys, bool>();
        }
        private void InitCreateSysTrayEntry()
        {
            m_KeepPopOuts = new MenuItem(Common.GetAppText(EAppTextLiterals.MenuSavePositions), new EventHandler(OnMenuSavePopOuts));
            m_SetPopOuts = new MenuItem(Common.GetAppText(EAppTextLiterals.MenuSetPositions), new EventHandler(OnMenuSetPopOuts));
            m_AvoidRenderMenuItem = new MenuItem(Common.GetAppText(EAppTextLiterals.MenuAvoidRendering), new EventHandler(OnMenuAvoidRendering));
            m_ExitMenuItem = new MenuItem(Common.GetAppText(EAppTextLiterals.MenuExit), new EventHandler(OnMenuExit));

            m_AvoidRenderMenuItem.Checked = false;

            m_NotifyIcon = new NotifyIcon();
            m_NotifyIcon.Icon = Properties.Resources.IconOn;
            m_NotifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { m_SetPopOuts, m_KeepPopOuts, m_AvoidRenderMenuItem, m_ExitMenuItem });
            m_NotifyIcon.Visible = true;
            m_NotifyIcon.Text = Common.GetAppText(EAppTextLiterals.AppTitle);
            m_NotifyIcon.BalloonTipText = "";
            m_NotifyIcon.ContextMenu.Popup += OnContextMenuPopup;

            m_NotifyIcon.ShowBalloonTip(7000, Common.GetAppText(EAppTextLiterals.AppTitle) + Common.GetAppText(EAppTextLiterals.AppStarted), Common.GetAppText(EAppTextLiterals.AppStartedHints), ToolTipIcon.Info);
        }
        private void InitFsConnection()
        {
            m_FlightSim = new FlightSimulator();
            UpdateFsConnection();
        }
        private void InitTimer()
        {
            m_Timer = new Timer() { Interval = 1000 };
            m_Timer.Tick += OnTimer;
            m_Timer.Start();
        }
        private void DeInitHooks()
        {
            if (m_GlobalHook != null)
            {
                m_GlobalHook.MouseDownExt -= OnGlobalHookMouseDownExt;
                m_GlobalHook.MouseMoveExt -= OnGlobalHookMouseMoveExt;
                m_GlobalHook.MouseUpExt -= OnGlobalHookMouseUpExt;

                m_GlobalHook.KeyPress -= OnGlobalHookKeyPress;
                m_GlobalHook.KeyDown -= OnGlobalHookKeyDown;
                //m_GlobalHook.KeyUp -= OnGlobalHookKeyUp;
                m_GlobalHook.Dispose();
                m_GlobalHook = null;
                m_GlobalKeyDict = null;
            }
        }
        private bool IsFsExists()
        {
            return m_FsConnectionState == EFsConnectionState.ConnectionEstablished;
        }
        private void UpdateFsConnection()
        {
            switch (m_FsConnectionState)
            {
                case EFsConnectionState.WaitForConnect:
                    if (m_FlightSim.Connect())
                    {
                        m_FsConnectionState = EFsConnectionState.ConnectionEstablished;
                        LoadSettings();
                    }
                    break;
                case EFsConnectionState.ConnectionEstablished:
                    if (!m_FlightSim.IsConnected)
                        m_FsConnectionState = EFsConnectionState.WaitForConnect;
                    break;
            }
        }
        private string GetCurPlane()
        {
            if (m_FlightSim.IsConnected)
                return m_FlightSim.PlaneInfo.Title;
            else
                return null;
        }
            
        private string GetSettingsFileName()
        {
            if (m_FlightSim.IsConnected)
            {
                var planeInfo = m_FlightSim.PlaneInfo;
                if (planeInfo.Title != null && planeInfo.Title.Length > 0)
                    return Path.GetTempPath() + planeInfo.Title + Common.GetAppText(EAppTextLiterals.FileSettings);
            }
            return null;
        }
        private bool LoadSettings()
        {
            var fileName = GetSettingsFileName();
            if (fileName != null && File.Exists(fileName))
            {
                var text = File.ReadAllText(fileName);
                m_PopOutEntries = JsonConvert.DeserializeObject<List<PopupEntry>>(text);
                foreach (var popout in m_PopOutEntries)
                    popout.WindowHandle = IntPtr.Zero;
                Log.Trace("load successful");
                return true;
            }
            Log.Trace("load failed");
            return false;
        }
        #endregion
        #region Events PopupMenu, Icon and Timer
        private void OnContextMenuPopup(object sender, EventArgs e)
        {
            m_SetPopOuts.Enabled = m_PopOutEntries.Count > 0;
        }
        private void OnIconClick(object sender, EventArgs e)
        {
            //TODO OnMenuAvoidRendering(this, null);
        }
        private void OnMenuSetPopOuts(object sender, EventArgs e)
        {
            if (LoadSettings())
            {
                m_NotifyIcon.ShowBalloonTip(5000, Common.GetAppText(EAppTextLiterals.AppTitle), Common.GetAppText(EAppTextLiterals.WaitForPositioning), ToolTipIcon.Info);
                List<PopupEntry> tempEntries = new List<PopupEntry>();
                DetectFsWindows(tempEntries, EFoundFSWindows.All);
                TryToSetPopOuts(tempEntries);
            }else
                m_NotifyIcon.ShowBalloonTip(5000, 
                    Common.GetAppText(EAppTextLiterals.AppTitle), 
                    string.Format(Common.GetAppText(EAppTextLiterals.NoPlaneProfileExists), GetCurPlane()), 
                    ToolTipIcon.Info);
        }
        private void OnMenuSavePopOuts(object sender, EventArgs e)
        {
            var fileName = GetSettingsFileName();
            if (fileName != null)
            {
                DetectFsWindows(m_PopOutEntries, EFoundFSWindows.All);
                var text = JsonConvert.SerializeObject(m_PopOutEntries);
                File.WriteAllText(fileName, text);
                text = string.Format(Common.GetAppText(EAppTextLiterals.PoitionsAreStored), m_PopOutEntries.Count);
                m_NotifyIcon.ShowBalloonTip(5000, Common.GetAppText(EAppTextLiterals.AppTitle), text, ToolTipIcon.Info);
            }
        }
        private void OnMenuAvoidRendering(object sender, EventArgs e)
        {
            m_AvoidRendering = !m_AvoidRendering;
            m_AvoidRenderMenuItem.Checked = m_AvoidRendering;
            if (m_AvoidRendering)
            {
                m_NotifyIcon.Icon = Properties.Resources.IconOff;
                m_NotifyIcon.ShowBalloonTip(5000, Common.GetAppText(EAppTextLiterals.AppTitle), Common.GetAppText(EAppTextLiterals.NoRenderActivity), ToolTipIcon.Info);
            }
            
            else
            {
                m_NotifyIcon.Icon = Properties.Resources.IconOn;
                m_NotifyIcon.ShowBalloonTip(5000, Common.GetAppText(EAppTextLiterals.AppTitle), Common.GetAppText(EAppTextLiterals.RenderActiv), ToolTipIcon.Info);
            }
        }
        private void OnMenuExit(object sender, EventArgs e)
        {
            //m_AvoidRendering = false;
            m_Timer.Stop();
            m_FlightSim?.Disconnect();
            m_NotifyIcon.Visible = false;
            DeInitHooks();
            Application.Exit();
        }
        private void OnTimer(object sender, EventArgs e)
        {
            if (m_AvoidRendering)
            {
                if (m_MsfsMainWindow != IntPtr.Zero)
                {
                    var activeWnd = WinApi.GetForegroundWindow();
                    if (activeWnd != m_MsfsMainWindow)
                        WinApi.PostMessage(m_MsfsMainWindow, WM.SYSCOMMAND, 0xF180, 0);
                }
                else
                    DetectFsWindows(null, EFoundFSWindows.MainWindow);
            }
            UpdateFsConnection();
        }
        #endregion
        #region Events Hooks
        private void OnGlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            var hwnd = WinApi.WindowFromPoint(e.Location);
            var caption = WinApi.GetWindowText(hwnd);
            var className = WinApi.GetClassName(hwnd);
            //Log.Trace("MouseButtons Down");
            if (className == Common.GetAppText(EAppTextLiterals.MsFsPopOutClassName))
            {
            }
        }
        private void OnGlobalHookMouseUpExt(object sender, MouseEventExtArgs e)
        {
            var hwnd = WinApi.WindowFromPoint(e.Location);
            var caption = WinApi.GetWindowText(hwnd);
            var className = WinApi.GetClassName(hwnd);
            //Log.Trace("MouseButtons Up");
            if (className == Common.GetAppText(EAppTextLiterals.MsFsPopOutClassName))
            {
                //Log.Trace("Mouse Up on Window Class: {0}\t Title: {1}", className, caption);
            }
        }
        private void OnGlobalHookMouseMoveExt(object sender, MouseEventExtArgs e)
        {
            //Log.Trace("MouseButtons Move");
        }
        private void OnGlobalHookKeyPress(object sender, KeyPressEventArgs e)
        {
            //Log.Trace("KeyPress: \t{0}", e.KeyChar);
        }
        private void OnGlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            if (!m_GlobalKeyDict.ContainsKey(e.KeyCode))
                m_GlobalKeyDict.Add(e.KeyCode, true);
            else
                m_GlobalKeyDict[e.KeyCode] = true;

            if (m_GlobalKeyDict.ContainsKey(Keys.LControlKey) && m_GlobalKeyDict[Keys.LControlKey] && m_GlobalKeyDict.ContainsKey(Keys.LWin) && m_GlobalKeyDict[Keys.LWin])
            {
                e.Handled = true;
                switch (e.KeyCode)
                {
                    case Keys.PrintScreen:
                        OnMenuSavePopOuts(this, null);
                        break;
                    case Keys.Home:
                        OnMenuSetPopOuts(this, null);
                        break;
                    case Keys.Insert:
                        OnMenuAvoidRendering(this.Tag, null);
                        break;
                    case Keys.Escape:
                        OnMenuExit(this.Tag, null);
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }
        }
        private void OnGlobalHookKeyUp(object sender, KeyEventArgs e)
        {
            if (m_GlobalKeyDict.ContainsKey(e.KeyCode))
                m_GlobalKeyDict[e.KeyCode] = false;
        }
        #endregion
        #region Functions
        private bool TryToSetPopOuts(List<PopupEntry> newList)
        {
            int proofed = 0;
            if (this.m_PopOutEntries.Count > 0)
            {
                for (int i = 0; i < newList.Count; i++)
                {
                    var curResIndex = -1;
                    switch (newList[i].WindowType)
                    {
                        case EMsfsWindowType.GaugeWindow:
                            curResIndex = FindPopOutByHash(newList[i].Histogram);
                            break;
                        case EMsfsWindowType.ToolBarWindows:
                            curResIndex = FindToolbarPopOut(newList[i].ToolbarWnd);
                            break;
                    }
                    if (curResIndex != -1)
                    {
                        PopupEntry entry = m_PopOutEntries[curResIndex];
                        entry.WindowHandle = newList[i].WindowHandle;
                        //WinApi.SetWindowPos(entry.WindowHandle, IntPtr.Zero, entry.Rectangle.Left, entry.Rectangle.Top, entry.Rectangle.Width, entry.Rectangle.Height, WinApi.SWP_NOZORDER);
                        if (SetPosAndSize(entry))
                            proofed++;
                    }
                }
                return proofed == this.m_PopOutEntries.Count;
            }
            return false;
        }
        private bool SetPosAndSize(PopupEntry entry)
        {
            int count = 0;
            bool same = true;
            do
            {
                WinApi.SetWindowPos(entry.WindowHandle, IntPtr.Zero, entry.Rectangle.Left, entry.Rectangle.Top, entry.Rectangle.Width, entry.Rectangle.Height, WinApi.SWP_NOZORDER);
                System.Threading.Thread.Sleep(20);
                var rect = WinApi.GetWindowRectangle(entry.WindowHandle);
                same = entry.Rectangle.Equals(rect);
                count++;
            } while (!same && count < Common.MaxNumPositioningAttempts);
            return count < Common.MaxNumPositioningAttempts;
        }
        private int FindToolbarPopOut(EToolbarWindows toolbarWnd)
        {
            for (int i = 0; i < m_PopOutEntries.Count; i++)
            {
                if (m_PopOutEntries[i].ToolbarWnd == toolbarWnd)
                    return i;
            }
            return -1;
        }
        private int FindPopOutByHash(double[] hash)
        {
            double curResSum = double.MaxValue;
            int curResIndex = -1;
            for (int i = 0; i < m_PopOutEntries.Count; i++)
            {
                if (m_PopOutEntries[i] != null)
                {
                    double curSum = 0;
                    var curHisto = m_PopOutEntries[i].Histogram;
                    if (curHisto == null)
                        continue;
                    for (int k = 0; k < hash.Length; k++)
                    {
                        curSum += Math.Abs(hash[k] - curHisto[k]);
                    }
                    if (curSum < curResSum)
                    {
                        curResSum = curSum;
                        curResIndex = i;
                    }
                }
            }
            return curResIndex;
        }
        private void DetectFsWindows(List<PopupEntry> list, EFoundFSWindows what)
        {
            EnumWindows(what);
            list?.Clear();
            for (int i = 0; i < m_FsPopOutWindows.Count; i++)
            {
                double[] histogram = null;
                var hwnd = m_FsPopOutWindows[i];
                var rect = WinApi.GetWindowRectangle(hwnd);
                var windowtype = GetWindowInfo(hwnd, out EToolbarWindows toolBarWindow);
                switch (windowtype)
                {
                    case EMsfsWindowType.CompundWindow:
                        continue;
                    case EMsfsWindowType.GaugeWindow:
                        histogram = m_HashBmpDict[hwnd].ToArray();
                        break;
                    case EMsfsWindowType.MainWindow:
                        m_MsfsMainWindow = hwnd;
                        continue;
                }
                list?.Add(new PopupEntry()
                {
                    WindowHandle = hwnd,
                    Rectangle = rect,
                    Histogram = histogram,
                    WindowType = windowtype,
                    ToolbarWnd = toolBarWindow,
                    Index = list.Count,
                });
            }
        }
        private void EnumWindows(EFoundFSWindows what)
        {
            m_HashBmpDict.Clear();
            m_MsfsMainWindow = IntPtr.Zero;
            m_FsPopOutWindows.Clear();
            WinApi.EnumWindows(new CallBack(CallBackEnumWindow), (int)what);
        }
        public bool CallBackEnumWindow(IntPtr hwnd, int lParam)
        {
            var windowtype = GetWindowInfo(hwnd, out EToolbarWindows toolBarWindow);
            if (windowtype != EMsfsWindowType.Other)
            {
                if ((EFoundFSWindows)lParam == EFoundFSWindows.MainWindow && windowtype == EMsfsWindowType.MainWindow)
                {
                    m_FsPopOutWindows.Add(hwnd);
                    return false;
                }
                if (windowtype == EMsfsWindowType.GaugeWindow)
                    GrabGaugeWindow(hwnd);
                m_FsPopOutWindows.Add(hwnd);
            }
            return true;
        }
        private void GrabGaugeWindow(IntPtr hwnd)
        {
            WinApi.SetWindowTopMost(hwnd);
            System.Threading.Thread.Sleep(10);
            BitmapFast bmp = BitmapFast.FromClientWindow(hwnd);
            List<double> histogram = Common.BuildImageHistogramCode(bmp);
            m_HashBmpDict.Add(hwnd, histogram);
            bmp.Dispose();
            WinApi.SetWindowTopMost(hwnd, 0, false);
        }
        public EMsfsWindowType GetWindowInfo(IntPtr hwnd, out EToolbarWindows toolBarWindow)
        {
            toolBarWindow = EToolbarWindows.None;
            string classname = WinApi.GetClassName(hwnd);
            if (classname == Common.GetAppText(EAppTextLiterals.MsFsPopOutClassName))
            {
                var title = WinApi.GetWindowText(hwnd);
                if (title.Length == 0)
                {
                    if (IsPopOutCompoundWindow(hwnd))
                        return EMsfsWindowType.CompundWindow;
                    else
                        return EMsfsWindowType.GaugeWindow;
                }
                else
                {
                    toolBarWindow = Common.FromToolbarTitle(title);
                    switch (toolBarWindow)
                    {
                        case EToolbarWindows.ATC:
                        case EToolbarWindows.FlightAssistant:
                        case EToolbarWindows.Camera:
                        case EToolbarWindows.CheckList:
                        case EToolbarWindows.BasicControls:
                        case EToolbarWindows.Fuel:
                        case EToolbarWindows.Navlog:
                        case EToolbarWindows.Objectives:
                        case EToolbarWindows.TravelTo:
                        case EToolbarWindows.VfrMap:
                        case EToolbarWindows.Weather:
                            return EMsfsWindowType.ToolBarWindows;
                        default:
                            return EMsfsWindowType.MainWindow;
                    }
                }
            }
            else
                return EMsfsWindowType.Other;
        }
        private bool IsPopOutCompoundWindow(IntPtr hwnd)
        {
            BitmapFast bmp = BitmapFast.FromClientWindow(hwnd);
            for (int x = 1; x < bmp.Width - 2; x++)
            {
                var pixUp = bmp.GetPixel(x, 1);
                var pixDown = bmp.GetPixel(x, bmp.Height - 2);
                if (!pixUp.EqualsColor(Color.Black) || !pixDown.EqualsColor(Color.Black))
                    return false;
            }
            for (int y = 1; y < bmp.Height - 1; y++)
            {
                var pixLeft = bmp.GetPixel(1, y);
                var pixRight = bmp.GetPixel(bmp.Width - 2, y);
                if (!pixLeft.EqualsColor(Color.Black) || !pixRight.EqualsColor(Color.Black))
                    return false;
            }
            return true;
        }
        #endregion
        #region Private Fields
        private MenuItem m_KeepPopOuts;
        private MenuItem m_SetPopOuts;
        private MenuItem m_AvoidRenderMenuItem = null;
        private MenuItem m_ExitMenuItem = null;
        private NotifyIcon m_NotifyIcon = null;
        private bool m_AvoidRendering = false;
        private Timer m_Timer;
        private IntPtr m_MsfsMainWindow;
        private EFsConnectionState m_FsConnectionState = EFsConnectionState.WaitForConnect;
        private IKeyboardMouseEvents m_GlobalHook;
        private List<IntPtr> m_FsPopOutWindows = new List<IntPtr>();
        private List<PopupEntry> m_PopOutEntries = new List<PopupEntry>();
        private Dictionary<Keys, bool> m_GlobalKeyDict;
        private Dictionary<IntPtr, List<double>> m_HashBmpDict = new Dictionary<IntPtr, List<double>>();
        private FlightSimulator m_FlightSim;
        #endregion
    }

}
