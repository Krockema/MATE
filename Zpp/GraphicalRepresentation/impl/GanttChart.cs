using System.Collections.Generic;
using Master40.DB.Data.Helper;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Newtonsoft.Json;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.Util.Graph;
using Zpp.Util.Graph.impl;

namespace Zpp.GraphicalRepresentation.impl
{
    public class GanttChart : IGanttChart
    {
        private readonly IDbMasterDataCache _dbMasterDataCache =
            ZppConfiguration.CacheManager.GetMasterDataCache();
        private readonly List<GanttChartBar> _ganttChartBars = new List<GanttChartBar>();

        public GanttChart(IEnumerable<ProductionOrderOperation> productionOrderOperations)
        {
            foreach (var productionOrderOperation in productionOrderOperations)
            {
                GanttChartBar ganttChartBar = new GanttChartBar();
                T_ProductionOrderOperation tProductionOrderOperation = productionOrderOperation.GetValue();

                ganttChartBar.operation = productionOrderOperation.GetId().ToString();
                ganttChartBar.operationId = tProductionOrderOperation.Id.ToString();
                if (tProductionOrderOperation.Resource == null)
                {
                    tProductionOrderOperation.Resource = _dbMasterDataCache
                        .M_ResourceGetById(new Id(tProductionOrderOperation.ResourceId
                            .GetValueOrDefault())).GetValue();
                }

                ganttChartBar.resource = tProductionOrderOperation.Resource.ToString();
                ganttChartBar.start = tProductionOrderOperation.Start.ToString();
                ganttChartBar.end = tProductionOrderOperation.End.ToString();

                ganttChartBar.groupId = productionOrderOperation.GetProductionOrderId().ToString();

                AddGanttChartBar(ganttChartBar);

            }
        }

        public GanttChart(IDirectedGraph<INode> orderOperationGraph)
        {
            Dictionary<Id, List<Interval>> groups = new Dictionary<Id, List<Interval>>();
            foreach (var graphNode in orderOperationGraph.GetNodes())
            {
                IScheduleNode node = graphNode.GetNode().GetEntity();
                if (node.GetType() != typeof(ProductionOrderOperation) && node.GetType() != typeof(PurchaseOrderPart))
                {
                    continue;
                }
                GanttChartBar ganttChartBar = new GanttChartBar();

                ganttChartBar.operation = node.GetId().ToString();
                ganttChartBar.operationId = node.GetId().ToString();
                ganttChartBar.resource = DetermineFreeGroup(groups,
                    new Interval(node.GetId(), node.GetStartTimeBackward(), node.GetEndTimeBackward())).ToString();

                ;
                ganttChartBar.start = node.GetStartTimeBackward().GetValue().ToString();
                ganttChartBar.end = node.GetEndTimeBackward().GetValue().ToString();

                ganttChartBar.groupId = ganttChartBar.resource;

                AddGanttChartBar(ganttChartBar);
            }
        }

        public static Id DetermineFreeGroup(Dictionary<Id, List<Interval>> groups, Interval givenInterval)
        {
            
            foreach (var groupId in groups.Keys)
            {
                bool occupied = false;
                foreach (var interval in groups[groupId])
                {
                    if (givenInterval.IntersectsExclusive(interval))
                    {
                        occupied = true;
                    }
                }

                if (occupied == false)
                {
                    groups[groupId].Add(givenInterval);
                    return groupId;
                }
            }

            Id newGroupId = IdGeneratorHolder.GetIdGenerator().GetNewId();
            groups.Add(newGroupId, new List<Interval>());
            groups[newGroupId].Add(givenInterval);
            return newGroupId;
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