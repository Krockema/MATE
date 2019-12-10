using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Zpp.DataLayer.impl.ProviderDomain.Wrappers;
using Zpp.DataLayer.impl.WrappersForCollections;

namespace Zpp.DataLayer.impl.ProviderDomain.WrappersForCollections
{
    public sealed class ProductionOrderOperations : CollectionWrapperWithStackSet<ProductionOrderOperation>
    {
        Dictionary<Id, Ids> _productionOrderToOperations = new Dictionary<Id, Ids>();
        
        public ProductionOrderOperations(IEnumerable<T_ProductionOrderOperation> list
            )
        {
            AddAll(Wrap(list));
        }

        public ProductionOrderOperations()
        {
        }

        private static List<ProductionOrderOperation> Wrap(
            IEnumerable<T_ProductionOrderOperation> list)
        {
            List<ProductionOrderOperation> productionOrderOperations =
                new List<ProductionOrderOperation>();
            foreach (var item in list)
            {
                productionOrderOperations.Add(new ProductionOrderOperation(item));
            }

            return productionOrderOperations;
        }
        
        public List<T_ProductionOrderOperation> GetAllAsT_ProductionOrderOperation()
        {
            List<T_ProductionOrderOperation> providers = new List<T_ProductionOrderOperation>();
            foreach (var operation in StackSet)
            {
                providers.Add( operation.GetValue());
            }

            return providers;
        }
        
        public override void Add(ProductionOrderOperation item)
        {
            if (base.Contains(item))
            {
                return;
            }
            Id productionOrderId = item.GetProductionOrderId();
            if (_productionOrderToOperations.ContainsKey(productionOrderId) == false)
            {
                _productionOrderToOperations.Add(productionOrderId, new Ids());
            }
            _productionOrderToOperations[productionOrderId].Add(item.GetId());
            base.Add(item);
        }

        public override void AddAll(IEnumerable<ProductionOrderOperation> items)
        {
            foreach (var item in items)
            {
                Add(item);    
            }
        }

        public override void Clear()
        {
            _productionOrderToOperations.Clear();
            base.Clear();
        }

        public override void Remove(ProductionOrderOperation t)
        {
            Id productionOrderId = t.GetProductionOrderId();
            _productionOrderToOperations[productionOrderId].Remove(t.GetId());
            
            base.Remove(t);
        }

        public Ids GetProductionOrderOperationsBy(Id productionOrderId)
        {
            return _productionOrderToOperations[productionOrderId];
        }
    }
}