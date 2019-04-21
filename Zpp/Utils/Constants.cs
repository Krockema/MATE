using System;
using System.Runtime.InteropServices;

namespace Zpp.Utils
{
    public static class Constants
    {
        public static readonly bool IsWindows = RuntimeInformation
            .IsOSPlatform(OSPlatform.Windows);

        public static readonly String DbConnectionZppWindows =
            "Server=(localdb)\\mssqllocaldb;Database=Zpp;Trusted_Connection=True;MultipleActiveResultSets=true";

        public static readonly String DbConnectionZppUnix =
            "Server=localhost,1433;Database=Zpp;MultipleActiveResultSets=true;User ID=SA;Password=123*Start#";
        
    }
}