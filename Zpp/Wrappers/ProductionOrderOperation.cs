using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.MachineDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp
{
    public class ProductionOrderOperation
    {
        private readonly T_ProductionOrderOperation _productionOrderOperation;
        private readonly IDbMasterDataCache _dbMasterDataCache;

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

        public List<Machine> GetMachines()
        {
            List<Machine> machines = new List<Machine>();
            return _dbMasterDataCache.M_MachineGetAll().Where(x =>
                x.GetMachineGroupId().GetValue().Equals(_productionOrderOperation.MachineGroupId)).ToList();
            return machines;
        }

        public override bool Equals(object obj)
        {
            ProductionOrderOperation productionOrderOperation = (ProductionOrderOperation) obj;
            return _productionOrderOperation.GetId().Equals(productionOrderOperation._productionOrderOperation.GetId());
        }

        public override int GetHashCode()
        {
            return _productionOrderOperation.Id.GetHashCode();
        }
    }
}