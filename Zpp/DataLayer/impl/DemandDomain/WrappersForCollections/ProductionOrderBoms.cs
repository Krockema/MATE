using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Zpp.DataLayer.impl.DemandDomain.WrappersForCollections
{
    /**
     * wraps the collection with all productionOrderBoms
     */
    public sealed class ProductionOrderBoms : Demands
    {
        Dictionary<Id, Ids> _operationToBom = new Dictionary<Id, Ids>();
        
        public ProductionOrderBoms(List<T_ProductionOrderBom> iDemands)
        {
            AddAll(ToDemands(iDemands));
        }

        public ProductionOrderBoms()
        {
        }

        public ProductionOrderBoms(List<Demand> demands)
        {
            AddAll(demands);
        }

        private static List<Demand> ToDemands(List<T_ProductionOrderBom> iDemands)
        {
            List<Demand> demands = new List<Demand>();
            foreach (var iDemand in iDemands)
            {
                demands.Add(new ProductionOrderBom(iDemand));
            }

            return demands;
        }

        public List<T_ProductionOrderBom> GetAllAsT_ProductionOrderBom()
        {
            List<T_ProductionOrderBom> productionOrderBoms = new List<T_ProductionOrderBom>();
            foreach (var demand in StackSet)
            {
                productionOrderBoms.Add((T_ProductionOrderBom)demand.ToIDemand());
            }
            return productionOrderBoms;
        }

        public override void Add(Demand item)
        {
            if (base.Contains(item))
            {
                return;
            }
            ProductionOrderBom productionOrderBom = (ProductionOrderBom) item;
            Id operationId = productionOrderBom.GetProductionOrderOperationId();
            if (_operationToBom.ContainsKey(operationId) == false)
            {
                _operationToBom.Add(operationId, new Ids());
            }
            _operationToBom[operationId].Add(productionOrderBom.GetId());
            base.Add(item);
        }

        public override void AddAll(IEnumerable<Demand> items)
        {
            foreach (var item in items)
            {
                Add(item);    
            }
        }

        public override void Clear()
        {
            _operationToBom.Clear();
            base.Clear();
        }

        public override void Remove(Demand t)
        {
            ProductionOrderBom productionOrderBom = (ProductionOrderBom) t;
            Id operationId = productionOrderBom.GetProductionOrderOperationId();
            _operationToBom[operationId].Remove(t.GetId());
            
            base.Remove(t);
        }

        public Ids GetProductionOrderBomsBy(ProductionOrderOperation operation)
        {
            return _operationToBom[operation.GetId()];
        }
    }
}