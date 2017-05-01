using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{
    interface IDemandForecast
    {
        void NetRequirement();
        void GrossRequirement();
    }

    internal class DemandForecast : IDemandForecast
    {
        private readonly MasterDBContext _context;

        internal DemandForecast(MasterDBContext context)
        {
            _context = context;
        }

        void IDemandForecast.NetRequirement()
        {
            var b =  _context.ArticleBoms;
            //Calculate net requirements from context
            //Access Article bill of materials and the order 
        }

        void IDemandForecast.GrossRequirement()
        {
            //calculate gross requirements from context
            //Access stock and net requirements

        }
    }
}
