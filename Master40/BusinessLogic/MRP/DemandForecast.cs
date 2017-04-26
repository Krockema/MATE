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

    class DemandForecast : IDemandForecast
    {
        private MasterDBContext _context;
        DemandForecast(MasterDBContext context)
        {
           _context = context;
        }

        void IDemandForecast.NetRequirement()
        {
           var b =  _context.ArticleBoms.Include(a => a.Article);
        }

        void IDemandForecast.GrossRequirement()
        {


        }
    }
}
