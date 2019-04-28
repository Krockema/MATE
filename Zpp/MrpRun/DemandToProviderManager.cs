using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp
{
    public class DemandToProviderManager
    {
        public void orderByUrgency(List<IDemand> demands)
        {
            demands.Sort((x,y)=> x.getDueTime().CompareTo(y.getDueTime()));
        }
    }
}