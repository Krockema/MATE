using System;
using System.Collections.Generic;
using Master40.DB.DataModel;
using Zpp.DemandDomain;

namespace Zpp.MachineDomain
{
    public class MachineManager : IMachineManager
    {
        public IMachine GetNextFreeMachine(ProductionOrderBoms productionOrderBoms)
        {
            List<Demand> operationsToPlan =
                productionOrderBoms.GetAll();
            while (operationsToPlan.Count > 0)
            {
                T_ProductionOrderBom productionOrderBom = (T_ProductionOrderBom) operationsToPlan[0].ToIDemand();
                if (productionOrderBom.ProductionOrderOperationId == null)
                {
                    operationsToPlan.RemoveAt(0);
                    continue;
                }
                
            }

            return null;
        }
    }
}