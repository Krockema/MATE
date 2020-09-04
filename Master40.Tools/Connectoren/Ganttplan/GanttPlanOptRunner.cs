using System.Diagnostics;

namespace Master40.Tools.Connectoren.Ganttplan
{
    public class GanttPlanOptRunner
    {
        private const string PATH_TO_GANTTOPTRUNNTER = "C:\\Program Files\\GANTTPLAN_V6\\GanttPlanOptRunner.exe";

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
        public static void RunOptAndExport()
        {
            Process process = Process.Start(PATH_TO_GANTTOPTRUNNTER);
            int id = process.Id;
            Process tempProc = Process.GetProcessById(id);

            tempProc.WaitForExit();

        }
    }
}
