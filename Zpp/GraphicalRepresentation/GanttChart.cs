using System;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Newtonsoft.Json;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.DbCache;

namespace Zpp.GraphicalRepresentation
{
    public class GanttChart : IGanttChart
    {
        private readonly List<GanttChartBar> _ganttChartBars = new List<GanttChartBar>();

        public GanttChart(List<ProductionOrderOperation> productionOrderOperations, IDbMasterDataCache dbMasterDataCache)
        {
            foreach (var productionOrderOperation in productionOrderOperations)
            {
                GanttChartBar ganttChartBar = new GanttChartBar();
                T_ProductionOrderOperation tProductionOrderOperation = productionOrderOperation.GetValue();
                
                ganttChartBar.operation = productionOrderOperation.ToString();
                ganttChartBar.operationId = tProductionOrderOperation.Id.ToString();
                if (tProductionOrderOperation.Machine == null)
                {
                    tProductionOrderOperation.Machine = dbMasterDataCache
                        .M_MachineGetById(new Id(tProductionOrderOperation.MachineId
                            .GetValueOrDefault())).GetValue();
                }

                ganttChartBar.resource = tProductionOrderOperation.Machine.ToString();
                ganttChartBar.start = tProductionOrderOperation.Start.ToString();
                ganttChartBar.end = tProductionOrderOperation.End.ToString();

                ganttChartBar.groupId = productionOrderOperation.GetProductionOrderId().ToString();

                AddGanttChartBar(ganttChartBar);
                
            }
        }

        public void AddGanttChartBar(GanttChartBar ganttChartBar)
        {
            _ganttChartBars.Add(ganttChartBar);
        }

        public List<GanttChartBar> GetAllGanttChartBars()
        {
            return _ganttChartBars;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(_ganttChartBars, Formatting.Indented);
        }
    }
}