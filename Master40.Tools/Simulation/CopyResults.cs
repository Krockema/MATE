using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
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
            var sim = productionDomainContext.Kpis.Last(); // i know not perfect ...
            foreach (var item in inMemmoryContext.Orders)
            {
                SimulationOrder set = new SimulationOrder();
                item.CopyPropertiesTo<IOrder>(set);
                set.SimulationConfigurationId = sim.SimulationConfigurationId;
                set.SimulationNumber = sim.SimulationNumber;
                set.SimulationType = sim.SimulationType;
                set.OriginId = item.Id;
                so.Add(set);
                
            }
            productionDomainContext.SimulationOrders.AddRange(so);
            productionDomainContext.SaveChanges();

            var simConfig = productionDomainContext.SimulationConfigurations.Single(s => s.Id == sim.SimulationConfigurationId);
            if (sim.SimulationType == SimulationType.Central)
            {
                simConfig.CentralRuns += 1;
            } else
            {
                simConfig.DecentralRuns += 1;
            }
            productionDomainContext.SaveChanges();
        }

    }
}
