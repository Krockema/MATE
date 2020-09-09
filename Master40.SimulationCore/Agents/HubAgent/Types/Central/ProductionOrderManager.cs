using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.GanttPlanModel;

namespace Master40.SimulationCore.Agents.HubAgent.Types.Central
{
    public class ProductionOrderManager
    {
        private List<GptblProductionorder> _productionorders { get; } = new List<GptblProductionorder>();

        private int _ganttCycle { get; }

        public ProductionOrderManager()
        {
            _ganttCycle = 0; // start at first cycle, ++ after each GanttPlan synchronization
        }

        public void UpdateOrCreate(GptblProductionorder productionorder)
        {
            var productionorderToUpdate = _productionorders.SingleOrDefault(x => x.ProductionorderId.Equals(productionorder.ProductionorderId));

            if (productionorderToUpdate != null)
            {
                _productionorders.Remove(productionorderToUpdate);
                _productionorders.Add(productionorder);
                return;
            }

            _productionorders.Add(productionorder);
        }

        /// <summary>
        /// To be implemented
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public GptblProductionorder GetNextProductionorder(string resourceName)
        {

            return null;
        }

    }
}
