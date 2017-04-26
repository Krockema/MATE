using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.BusinessLogic.MRP
{
    interface IDemandForecast
    {
        void NetRequirement();
        void GrossRequirement();
    }

    class DemandForecast : IDemandForecast
    {
        void IDemandForecast.NetRequirement()
        {

        }

        void IDemandForecast.GrossRequirement()
        {


        }
    }
}
