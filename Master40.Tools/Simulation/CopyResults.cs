using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.Tools.Simulation
{
    public static class CopyResults
    {
        public static void Copy(InMemoryContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            List<Kpi> kpis = new List<Kpi>();
            foreach (var item in inMemmoryContext.Kpis)
            {
                kpis.Add(item.CopyDbPropertiesWithoutId());
            }
            productionDomainContext.Kpis.AddRange(kpis);
            productionDomainContext.SaveChanges();
        }

    }
}
