using System;
using System.Runtime.InteropServices;

namespace Zpp.Utils
{
    public static class Constants
    {
        public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static readonly String DbConnectionZppWindows =
            "Server=(localdb)\\mssqllocaldb;Database=Zpp;Trusted_Connection=True;MultipleActiveResultSets=true";

        // TODO: the random is a workaround, remove this if drop database query in Dispose() added
        public static String DbConnectionZppUnix()
        {
            return $"Server=localhost,1433;Database=zpp;" +
                $"MultipleActiveResultSets=true;User ID=SA;Password=123*Start#;Persist Security Info=True";
        }
    }
}