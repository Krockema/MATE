using System;
using System.Diagnostics;

namespace Master40.Tools.Connectoren.Ganttplan
{
    public class GanttPlanOptRunner
    {
        private const string PATH_TO_GANTTOPTRUNNTER = "C:\\Program Files\\GANTTPLAN\\GanttPlanOptRunner.exe";

        public const string Init = "Init";
        /// <summary>
        /// Start default OptRun and Export Productionorder results to GanttPlanDB
        /// GanttPlanApics.GPInitInstanceEx(cPathApp: "C:\\Program Files\\GANTTPLAN_V6\\Init", cPathUser: "C:\\Users\\Administrator");
        //GanttPlanApics.GPLic();
        //GanttPlanApics.GPImport("15");
        //GanttPlanApics.GPOptInit("");
        //GanttPlanApics.GPOptRun();
        //GanttPlanApics.GPExport(1);
        //GanttPlanApics.GPExitInstance();
        /// </summary>
        // mode: "Continuous" or "Init"

        public static void Inizialize()
        {
            RunOptAndExport(Init);
        }

        public static void RunOptAndExport(string mode)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(PATH_TO_GANTTOPTRUNNTER, mode);
            Process process = Process.Start(processStartInfo);
            
            int id = process.Id;
            Process tempProc = Process.GetProcessById(id);

            tempProc.WaitForExit();
        }
    }
}
