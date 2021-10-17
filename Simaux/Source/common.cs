using FastBitmap;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Simaux
{
    public enum EFsConnectionState
    {
        WaitForConnect,
        ConnectionEstablished
    }
    public enum EFoundFSWindows
    {
        All,
        MainWindow,
    }
    public enum EMsfsWindowType
    {
        MainWindow,
        GaugeWindow,
        ToolBarWindows,
        CompundWindow,
        Other,
    }
    public enum EToolbarWindows
    {
        None,
        ATC,
        FlightAssistant,
        Camera,
        CheckList,
        BasicControls,
        Fuel,
        Navlog,
        Objectives,
        TravelTo,
        VfrMap,
        Weather,
    }
    public enum EAppTextLiterals
    {
        AppTitle,
        AppStarted,
        AppStartedHints,
        MsFsPopOutClassName,
        WaitForPositioning,
        PoitionsAreStored,
        NoRenderActivity,
        RenderActiv,
        MenuSavePositions,
        MenuSetPositions, 
        MenuAvoidRendering,
        MenuExit,
        FileSettings,
        NoPlaneProfileExists,
        DebugPopupEntryToString,
    }

    public static class Common
    {
        public static string[] ToolbarTitles = { "None", "ATC", "FLIGHT ASSISTANT", "CAMERA", "CHECKLIST", "BASIC CONTROLS", "FUEL", "NAVLOG", "OBJECTIVES", "TRAVEL TO", "VFR MAP", "WEATHER" };
        public static string[] AppTextLiterals =
        {
            "Simaux",
            " successful started",
            "\r\nUse popup menu in systray or the keyboard short cuts",
            "AceApp",
            "Please wait for Windows Positioning",
            "The Positions and sizes of {0} MSFS popup Windows are stored",
            "When MSFS is not active, the CPU and GPU are no longer used by the program.",
            "If MSFS isn't active, CPU and GPU are used anyway",
            "Save PopOut Positions\t Win+Ctrl+Print",
            "Set PopOut Positions\t Win+Ctrl+Home",
            "Avoid rendering when inactive\t Win+Ctrl+Insert",
            "Exit\t Win+Ctrl+Escape",
            "MsfsSettings.dat",
            "No profile exists for the airplane: {0}",
            "{0}  Type: {1}\tToolbar: {2}\tInPos: {3}\tExists: {4}",

        };
        public static int MaxNumPositioningAttempts = 255;
        public static string GetAppText(EAppTextLiterals what)
        {
            return AppTextLiterals[(int)what];
        }
        public static string GetToolbarTitle(EToolbarWindows toolbarWindow)
        {
            return ToolbarTitles[(int)toolbarWindow];
        }
        public static EToolbarWindows FromToolbarTitle(string title)
        {
            for (int i = 0; i < ToolbarTitles.Length; i++)
            {
                if (title == ToolbarTitles[i])
                    return (EToolbarWindows)i;
            }
            return EToolbarWindows.None;
        }
        public static double[] GetBrightnessHistogram(BitmapFast bmp)
        {
            double[] res = new double[256];
            double numPixel = 0;
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var br = bmp.GetBrightness(x, y);
                    if (br > 0 && br < 255)
                    {
                        numPixel++;
                        res[br]++;
                    }
                }
            }
            for (int i = 0; i < res.Length; i++)
                res[i] /= numPixel;
            return res;
        }
        public static List<double> BuildImageHistogramCode(BitmapFast bmp)
        {
            int numRegionsX = 8;
            int numRegionsY = 8;
            int regWidth = bmp.Width / numRegionsX;
            int regHeight = bmp.Height / numRegionsY;
            var bMax = byte.MinValue;
            var bMin = byte.MaxValue;

            List<double> result = new List<double>(numRegionsX * numRegionsY);
            for (int y = 0; y < numRegionsY; y++)
            {
                for (int x = 0; x < numRegionsX; x++)
                {
                    var curRegion = new Rectangle(x * regWidth, y * regHeight, regWidth, regHeight);
                    var value = bmp.GetBrightness(curRegion);
                    result.Add(value);
                    if (value < bMin)
                        bMin = value;
                    if (value > bMax)
                        bMax = value;
                }
            }
            return result;
        }
        public static bool IsInstanceAlreadyRunning()
        { 
            var exists = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (exists)
            {
                MessageBox.Show("MsfsCommon is already running. Do not start the application a second time.", "MsfsCommon", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            return false;
        }
    }
}
