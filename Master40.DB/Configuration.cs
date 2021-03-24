using System.Configuration;

namespace Master40.DB
{
    public static class DataBaseConfiguration
    {
        public static bool TrackObjects = false;
        public static string SQLServer = ConfigurationManager.AppSettings["SQLServer"];
        public static string DefaultLocalHostConnection = ConfigurationManager.AppSettings["DefaultLocalHostConnection"];
        public static string HubConnection = ConfigurationManager.AppSettings["HubConnection"];
        public static string HubChannel = ConfigurationManager.AppSettings["HubChannel"];
    }

}