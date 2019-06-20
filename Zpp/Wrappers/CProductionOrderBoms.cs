using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.Entities
{
    /**
     * A wrapper for the productionOrderBoms, C stands for collection
     */
    public class CProductionOrderBoms
    {
        private List<T_ProductionOrderBom> _productionOrderBoms;
        private T_ProductionOrder _parentProductionOrder;
        
        public CProductionOrderBoms(List<T_ProductionOrderBom> productionOrderBoms, T_ProductionOrder parentProductionOrder)
        {
            _productionOrderBoms = productionOrderBoms;
            _parentProductionOrder = parentProductionOrder;
        }
    }
}