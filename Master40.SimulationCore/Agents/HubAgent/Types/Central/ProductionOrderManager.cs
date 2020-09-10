using System;
using System.Collections.Generic;
using System.Data.HashFunction.xxHash;
using System.Linq;
using System.Text;
using Master40.DB.GanttPlanModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ProductionOrderManager
    {
        public List<GptblProductionorderOperationActivityResourceInterval> _resourceActivityInterval { get; private set; } = new List<GptblProductionorderOperationActivityResourceInterval>();

        public int _ganttPlanningId { get; private set; }

        public ProductionOrderManager()
        {
            _ganttPlanningId = 0; // start at first cycle, ++ after each GanttPlan synchronization
        }

        public void Update(List<GptblProductionorderOperationActivityResourceInterval> resourceActivityInterval)
        {
            _resourceActivityInterval = resourceActivityInterval;
            IncrementGanttPlanPlanningId();
        }

        /// <summary>
        /// To be implemented
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public GptblProductionorderOperationActivityResourceInterval GetNextInterval(string resourceId)
        {
            var resourceInterval = _resourceActivityInterval.
                    OrderBy(x => x.DateFrom)
                    .FirstOrDefault(x => x.ResourceId.Equals(resourceId));

           
            return resourceInterval;

        }

        
        public int IncrementGanttPlanPlanningId()
        {
            return _ganttPlanningId++;
        }

        internal List<GptblProductionorderOperationActivityResourceInterval> GetIntervalsActivity(GptblProductionorderOperationActivity nextActivityForResource)
        {
            List<GptblProductionorderOperationActivityResourceInterval> resourceIntervals = new List<GptblProductionorderOperationActivityResourceInterval>();

            resourceIntervals = _resourceActivityInterval.Where(x =>
                x.ProductionorderId.Equals(nextActivityForResource.ProductionorderId)
                && x.OperationId.Equals(nextActivityForResource.OperationId)
                && x.ActivityId.Equals(nextActivityForResource.ActivityId)).ToList();

            return resourceIntervals;
        }
    }
}
