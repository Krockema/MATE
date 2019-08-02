using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp
{
    public class ProductionOrderOperation
    {
        private T_ProductionOrderOperation _productionOrderOperation;

        public ProductionOrderOperation(T_ProductionOrderOperation productionOrderOperation)
        {
            _productionOrderOperation = productionOrderOperation;
        }

        public static T_ProductionOrderOperation CreateProductionOrderOperation(M_ArticleBom articleBom, Provider parentProductionOrder)
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
            productionOrderOperation.ProductionOrder = (T_ProductionOrder)parentProductionOrder.ToIProvider();
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
    }
}