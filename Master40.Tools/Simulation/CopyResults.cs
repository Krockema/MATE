using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Interfaces;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.Tools.Simulation
{
    public static class CopyResults
    {
        public static void Copy(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            List<Kpi> kpis = new List<Kpi>();
            foreach (var item in inMemmoryContext.Kpis)
            {
                kpis.Add(item.CopyDbPropertiesWithoutId());
            }
            productionDomainContext.Kpis.AddRange(kpis);
            productionDomainContext.SaveChanges();


            List<SimulationWorkschedule> sw = new List<SimulationWorkschedule>();
            foreach (var item in inMemmoryContext.SimulationWorkschedules)
            {
                sw.Add(item.CopyDbPropertiesWithoutId());
            }

            productionDomainContext.SimulationWorkschedules.AddRange(sw);
            productionDomainContext.SaveChanges();

            List<SimulationOrder> so = new List<SimulationOrder>();
            var simId = productionDomainContext.Kpis.Last(); // i know not perfect ...
            foreach (var item in inMemmoryContext.Orders)
            {
                SimulationOrder set = new SimulationOrder();
                item.CopyPropertiesTo<IOrder>(set);
                set.SimulationConfigurationId = simId.SimulationConfigurationId;
                set.SimulationNumber = simId.SimulationNumber;
                set.SimulationType = simId.SimulationType;
                so.Add(set);
                
            }
            productionDomainContext.SimulationOrders.AddRange(so);
            productionDomainContext.SaveChanges();

        }

    }
}
