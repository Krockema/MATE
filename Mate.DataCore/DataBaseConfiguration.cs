using System.Configuration;

namespace Mate.DataCore
{
    public static class DataBaseConfiguration
    {
        public static bool TrackObjects = false;
        public static string SQLServer = ConfigurationManager.AppSettings["SQLServer"];
        public static string DefaultLocalHostConnection = ConfigurationManager.AppSettings["DefaultLocalHostConnection"];
        public static string HubConnection = ConfigurationManager.AppSettings["HubConnection"];
        public static string HubChannel = ConfigurationManager.AppSettings["HubChannel"];
        public static string MateDb = "MateDb";
        public static string MateResultDb = "MateResultDb";
        public static string MateHangfireDb = "MateHangfire";
        public static string GP = ConfigurationManager.AppSettings["GanttPlanDataBase"];
    }

}