using Zpp.DataLayer;
using Zpp.DataLayer.impl;

namespace Zpp
{
    public static class ZppConfiguration
    {
        public static readonly ICacheManager CacheManager = new CacheManager();

        // if true, no log info or files are created (e.g. PrintState() etc.)
        public static bool IsInPerformanceMode = true;
        
    }
}