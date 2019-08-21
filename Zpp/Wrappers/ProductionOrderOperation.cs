using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.MachineDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp
{
    public class ProductionOrderOperation : INode
    {
        private readonly T_ProductionOrderOperation _productionOrderOperation;
        private readonly IDbMasterDataCache _dbMasterDataCache;
        private Priority _priority = null;

        public ProductionOrderOperation(T_ProductionOrderOperation productionOrderOperation,
            IDbMasterDataCache dbMasterDataCache)
        {
            _productionOrderOperation = productionOrderOperation;
            _dbMasterDataCache = dbMasterDataCache;
        }

        public static T_ProductionOrderOperation CreateProductionOrderOperation(
            M_ArticleBom articleBom, Provider parentProductionOrder)
        {
            T_ProductionOrderOperation productionOrderOperation = new T_ProductionOrderOperation();
            productionOrderOperation = new T_ProductionOrderOperation();
            // TODO: add not only entities but also the ids !!! --> only ids should be enough???
            productionOrderOperation.Name = articleBom.Operation.Name;
            productionOrderOperation.HierarchyNumber = articleBom.Operation.HierarchyNumber;
            productionOrderOperation.Duration = articleBom.Operation.Duration;
            // Tool has no meaning yet, ignore it
            productionOrderOperation.MachineTool = articleBom.Operation.MachineTool;
            productionOrderOperation.MachineToolId = articleBom.Operation.MachineToolId;
            productionOrderOperation.MachineGroup = articleBom.Operation.MachineGroup;
            productionOrderOperation.MachineGroupId = articleBom.Operation.MachineGroupId;
            productionOrderOperation.ProducingState = ProducingState.Created;
            productionOrderOperation.ProductionOrder =
                (T_ProductionOrder) parentProductionOrder.ToIProvider();
            productionOrderOperation.ProductionOrderId =
                productionOrderOperation.ProductionOrder.Id;

            // TODO: external Algo needed

            // for machine utilisation
            // productionOrderOperation.Machine,

            // for simulation
            // Start, End,

            // for backward scheduling
            // StartBackward, EndBackward,

            // for forward scheduling
            // StartForward, EndForward,

            return productionOrderOperation;
        }

        public T_ProductionOrderOperation GetValue()
        {
            return _productionOrderOperation;
        }

        public Id GetMachineGroupId()
        {
            return new Id(_productionOrderOperation.MachineGroupId);
        }

        public void SetMachine(Machine machine)
        {
            _productionOrderOperation.Machine = machine.GetValue();
        }

        public List<Machine> GetMachines(IDbTransactionData dbTransactionData)
        {
            return dbTransactionData.GetAggregator().GetMachinesOfProductionOrderOperation(this);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType()!=typeof(ProductionOrderOperation))
            {
                return false;
            }
            ProductionOrderOperation productionOrderOperation = (ProductionOrderOperation) obj;
            return _productionOrderOperation.GetId().Equals(productionOrderOperation._productionOrderOperation.GetId());
        }

        public override int GetHashCode()
        {
            return _productionOrderOperation.Id.GetHashCode();
        }

        public HierarchyNumber GetHierarchyNumber()
        {
            return new HierarchyNumber(_productionOrderOperation.HierarchyNumber);
        }

        public DueTime GetDuration()
        {
            return new DueTime(_productionOrderOperation.Duration);
        }

        public Id GetId()
        {
            return _productionOrderOperation.GetId();
        }

        public NodeType GetNodeType()
        {
            return NodeType.Operation;
        }

        public INode GetEntity()
        {
            return this;
        }

        public string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            return $"{_productionOrderOperation.Name}";
        }

        public string GetJsonString(IDbTransactionData dbTransactionData)
        {
            throw new System.NotImplementedException();
        }

        public Id GetProductionOrderId()
        {
            if (_productionOrderOperation.ProductionOrderId == null)
            {
                return null;
            }
            return new Id(_productionOrderOperation.ProductionOrderId.GetValueOrDefault());
        }

        public override string ToString()
        {
            return $"{_productionOrderOperation.GetId()}: {_productionOrderOperation.Name}";
        }

        public void SetPriority(Priority priority)
        {
            _priority = priority;
        }

        public Priority GetPriority()
        {
            return _priority;
        }
    }
}