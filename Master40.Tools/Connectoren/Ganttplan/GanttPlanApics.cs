using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Master40.Tools.Connectoren.Ganttplan
{
    /// <summary>
    /// GanttPlan API Wrapper to access Ganttplan functionality
    /// </summary>
    class GanttPlanApics
    {
        [DllImport(dllName: "gpDll.dll", EntryPoint = "GPGetVersion", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr GPGetVersionAsPtr();

        /// <summary>
        /// Returns the currently used gpDLL.dll Version
        /// </summary>
        /// <returns>String: Version description</returns>
        public static String GPGetVersion() {
            IntPtr ptr = GPGetVersionAsPtr();
            return Marshal.PtrToStringAnsi(ptr: ptr);
        }

        /// <summary>
        /// Initializes the required Data Structures for Planning, In and Export 
        /// </summary>
        /// <returns>sucress or failure</returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPInitInstance();

        /// <summary>
        /// Initializes the required Data Structures for Planning, In and Export 
        /// </summary>
        /// <param name="cPathApp">Path for Ganttplan required Data (.config, lang, ...)</param>
        /// <param name="cPathUser">Path for relevant export/output</param>
        /// <returns>sucress or failure</returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPInitInstanceEx(String cPathApp, String cPathUser);

        /// <summary>
        /// Licence check
        /// </summary>
        /// <param name="bShowLicInfo">If set to true, it shows full licence Information after successfull confirm</param>
        /// <param name="bDisableDefaultScreen">If set to true, id disables the user input screeen for licence input</param>
        /// <returns>sucress or failure</returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPLic(bool bShowLicInfo = false, bool bDisableDefaultScreen = false);

        /// <summary>
        /// Enables to add pairs of strings to the current import configuration, the pairs will replace parts in the configured SQL-Querry 
        /// Diese Funktion ermöglicht es, Paare von Zeichen(ketten) bereitzustellen, die im folgenden Import in den konfigurierten SQL-Abfragen miteinander ersetzt werden.
        /// </summary>
        /// <remarks>GPLic has to be successfully called before</remarks>
        /// <param name="cOld">No "!, string to replace</param>
        /// <param name="cNew">No "!, string to insert</param>
        /// <returns></returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPAddImportQueryArgument(String cOld, String cNew);

        /// <summary>
        /// Imports the data from the configured Data source
        /// </summary>
        /// <remarks>GPLic has to be successfully called before</remarks>
        /// <param name="cSessionID">can be Null, then the current date is used.</param>
        /// <returns></returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPImport(String cSessionID);

        /// <summary>
        /// Imports the data from the configured Data source
        /// </summary>
        /// <remarks>GPLic has to be successfully called before</remarks>
        /// <param name="cSessionID">can be Null, then the current date is used.</param>
        /// <param name="cFileName">Full path to the scenario</param>
        /// <returns></returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPImportScenario(String cSessionID, String cFileName);

        /// <summary>
        /// Initializes the planning and checks the licence
        /// </summary>
        /// <remarks>GPImport has to be successfully called before</remarks>
        /// <param name="cPlanParamID">If null, the first configuration entry is used</param>
        /// <returns></returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPOptInit(String cPlanParamID);

        /// <summary>
        /// Starts the planning
        /// </summary>
        /// <remarks>GPOptInit has to be successfully called before</remarks>
        /// <returns></returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPOptRun();

        /// <summary>
        /// Creates an individual CSV Report.
        /// </summary>
        /// <param name="nResult">specifies which planning result shall be returned, -1 Production Plan, 0 Result ... Result Count -1 for last Result</param>
        /// <param name="nType">specifies which report shall be returned</param>
        /// <param name="cCols">specifies all columns of the report to create seperated by the configurated seperator (Default: | ), on Null all columns will be calculated</param>
        /// <param name="cObjectID">If set to null it creates the report for all available Objects</param>
        /// <param name="cObjectType">If more then one object type is available it has to be specified here</param>
        /// <param name="nIntervalType">If more then one intervall Types are available it hase to be specified here 
        /// 0 = Hours
        /// 1 = Days
        /// 2 = Weeks 
        /// 3 = Months </param>
        /// <returns></returns>
        /// FILE OUT after Cols, IS MAYBE MISSING AS PARAMETER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// FILE OUT after Cols, IS MAYBE MISSING AS PARAMETER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// FILE OUT after Cols, IS MAYBE MISSING AS PARAMETER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        /// FILE OUT after Cols, IS MAYBE MISSING AS PARAMETER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPReport(int nResult, int nType, /* String cFileOut , */ String cCols, String cObjectID, String cObjectType, int nIntervalType);

        /// <summary>
        /// Exports the Results into the configured data-sink
        /// </summary>
        /// <param name="nResultsMax">Configures which result shall be exportet, if set to 0 all results will be exportet.</param>
        /// <returns></returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPExport(int nResultsMax);

        /// <summary>
        /// Saves the result to an *.gpsx file
        /// </summary>
        /// <param name="nResult">specifies the Index of the result which should be exported</param>
        /// <param name="cFileName">specifies the full path</param>
        /// <returns></returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPSaveScenario(int nResult, String cFileName);

        /// <summary>
        /// Frees allocated memory space
        /// </summary>
        /// <returns></returns>
        [DllImport(dllName: "gpDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GPExitInstance();
    }
}

