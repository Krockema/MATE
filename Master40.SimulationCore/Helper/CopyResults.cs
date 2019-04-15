using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.ReportingModel;

namespace Master40.SimulationCore.Helper
{
    public static class CopyResults
    {
        public static void Copy(MasterDBContext inMemmoryContext, ProductionDomainContext productionDomainContext, ResultContext inMemmoryResultContext, ResultContext productionResultContext, int simId, int simNo, SimulationType simType)
        {
            ExtractKpis(inMemmoryResultContext, productionResultContext);
            ExtractOperations(inMemmoryResultContext, productionResultContext);
            ExtractStockExchanges(inMemmoryContext, productionResultContext);
            ExtractSimulationOrders(inMemmoryContext, productionResultContext, simId, simNo, simType);

            var simConfig = productionResultContext.SimulationConfigurations.Single(s => s.Id == simId);
            if (simType == SimulationType.Central) {
                simConfig.CentralRuns += 1;
                simConfig.CentralIsRunning = false;
            } else {
                simConfig.DecentralRuns += 1;
                simConfig.DecentralIsRunning = false;
            }
            productionDomainContext.SaveChanges();



        }

        public static void ExtractSimulationOrders(MasterDBContext inMemmoryContext, ResultContext productionDomainContext, 
            int simId, int simNo, SimulationType simType)
        {
            List<SimulationOrder> so = new List<SimulationOrder>();
            foreach (var item in inMemmoryContext.CustomerOrders.ToList())
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

        private static void ExtractKpis(ResultContext inMemmoryContext, ResultContext productionDomainContext)
        {
            //List<Kpi> kpis = new List<Kpi>();
            //inMemmoryContext.Kpis.AsNoTracking().ToList();
            // foreach (var item in inMemmoryContext.Kpis.ToList())
            // {
            //     kpis.Add(item.CopyDbPropertiesWithoutId());
            // }
            var kpis = inMemmoryContext.Kpis.AsNoTracking().ToList().Select(x => { x.Id = 0; return x; }).ToList();


            productionDomainContext.Kpis.AddRange(kpis);
            productionDomainContext.SaveChanges();
        }

        private static void ExtractOperations(ResultContext inMemmoryContext, ResultContext productionDomainContext)
        {
            var sw = inMemmoryContext.SimulationOperations.AsNoTracking().ToList().Select(x => { x.Id = 0; return x; }).ToList();
            productionDomainContext.SimulationOperations.AddRange(sw);
            productionDomainContext.SaveChanges();
        }

        private static void ExtractStockExchanges(MasterDBContext inMemmoryContext, ResultContext productionDomainContext)
        {
            var se = inMemmoryContext.StockExchanges.AsNoTracking().ToList().Select(x => { x.Id = 0; return x; }).ToList();

            List<StockExchange> exchangeList = new List<StockExchange>();
            foreach (var item in se)
            {
                StockExchange set = new StockExchange();
                item.CopyPropertiesTo<IStockExchange>(set);
                exchangeList.Add(set);

            }

            productionDomainContext.StockExchanges.AddRange(exchangeList);
            productionDomainContext.SaveChanges();
        }
    }
}
