using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Master40.Tools.Connectoren.Ganttplan
{
    /// <summary>
    /// GanttPlan API Wrapper to access Ganttplan functionality
    /// </summary>
    /// 
    public class GanttPlanApics
    {
      
        private const string PATH_TO_GANTT = @"gp-c-api.dll";

        [DllImport(PATH_TO_GANTT, EntryPoint = "GPGetVersion", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr GPGetVersionAsPtr();

        public static String GPGetVersion()
        {
            IntPtr ptr = GPGetVersionAsPtr();
            return Marshal.PtrToStringAnsi(ptr);
        }
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPInitInstance();
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPInitInstanceEx(String cPathApp, String cPathUser);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPLic(bool bShowLicInfo = false, bool bDisableDefaultScreen = false);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPAddImportQueryArgument(String cOld, String cNew);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPImport(String cSessionID);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPImportScenario(String cSessionID, String cFileName);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPOptInit(String cPlanParamID);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPOptRun();
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPReport(int nResult, int nType, String cCols, String cObjectID, String cObjectType, int nIntervalType);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPExport(int nResultsMax);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPSaveScenario(int nResult, String cFileName);
        [DllImport(PATH_TO_GANTT, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPExitInstance();

    }
}

