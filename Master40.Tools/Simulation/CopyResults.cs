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
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Master40.Tools.Simulation
{
    public static class CopyResults
    {
        public static void Copy(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext, int simId, int simNo, SimulationType simType)
        {
            ExtractKpis(inMemmoryContext, productionDomainContext);
            ExtractWorkSchedules(inMemmoryContext, productionDomainContext);
            ExtractStockExchanges(inMemmoryContext, productionDomainContext);
            ExtractSimulationOrders(inMemmoryContext, productionDomainContext, simId, simNo, simType);

            var simConfig = productionDomainContext.SimulationConfigurations.Single(s => s.Id == simId);
            if (simType == SimulationType.Central) {
                simConfig.CentralRuns += 1;
                simConfig.CentralIsRunning = false;
            } else {
                simConfig.DecentralRuns += 1;
                simConfig.DecentralIsRunning = false;
            }
            productionDomainContext.SaveChanges();



        }

        public static void ExtractSimulationOrders(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext, 
            int simId, int simNo, SimulationType simType)
        {
            List<SimulationOrder> so = new List<SimulationOrder>();
            foreach (var item in inMemmoryContext.Orders.ToList())
            {
                SimulationOrder set = new SimulationOrder();
                item.CopyPropertiesTo<IOrder>(set);
                set.SimulationConfigurationId = simId;
                set.SimulationNumber = simNo;
                set.SimulationType = simType;
                set.OriginId = item.Id;
                so.Add(set);

            }
            productionDomainContext.SimulationOrders.AddRange(so);
            productionDomainContext.SaveChanges();
        }

        private static void ExtractKpis(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            //List<Kpi> kpis = new List<Kpi>();
            //inMemmoryContext.Kpis.AsNoTracking().ToList();
            // foreach (var item in inMemmoryContext.Kpis.ToList())
            // {
            //     kpis.Add(item.CopyDbPropertiesWithoutId());
            // }
            var kpis = inMemmoryContext.Kpis.ToList().Select(x => { x.Id = 0; return x; }).ToList();


            productionDomainContext.Kpis.AddRange(kpis);
            productionDomainContext.SaveChanges();
        }

        private static void ExtractWorkSchedules(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
           List<SimulationWorkschedule> sw = new List<SimulationWorkschedule>();
           foreach (var item in inMemmoryContext.SimulationWorkschedules.ToList())
           {
               sw.Add(item.CopyDbPropertiesWithoutId());
           }
           
            productionDomainContext.SimulationWorkschedules.AddRange(sw);
            productionDomainContext.SaveChanges();
        }

        private static void ExtractStockExchanges(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext)
        {
            List<StockExchange> se = new List<StockExchange>();
            
            foreach (var item in inMemmoryContext.StockExchanges.ToList())
            {
                se.Add(item.CopyDbPropertiesWithoutId());
            }
            productionDomainContext.StockExchanges.AddRange(se);
            productionDomainContext.SaveChanges();
        }
    }
}
