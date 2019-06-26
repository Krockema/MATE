using Master40.DB.DataModel;
using Master40.DB.Enums;

namespace Zpp
{
    public class ProductionOrderOperation
    {
        private T_ProductionOrderOperation _productionOrderOperation;
        
        public ProductionOrderOperation(M_ArticleBom articleBom)
        {
            if (articleBom.ArticleChild.ToBuild)
            {

                _productionOrderOperation = new T_ProductionOrderOperation();
                // TODO: add not only entities but also the ids !!! --> only ids should be enough???
                _productionOrderOperation.Name = articleBom.Operation.Name;
                _productionOrderOperation.HierarchyNumber = articleBom.Operation.HierarchyNumber;
                _productionOrderOperation.Duration = articleBom.Operation.Duration;
                _productionOrderOperation.MachineTool = articleBom.Operation.MachineTool;
                _productionOrderOperation.MachineToolId = articleBom.Operation.MachineToolId;
                _productionOrderOperation.MachineGroup = articleBom.Operation.MachineGroup;
                _productionOrderOperation.MachineGroupId = articleBom.Operation.MachineGroupId;
                _productionOrderOperation.ProducingState = ProducingState.Created;

                // TODO: external Algo needed

                // for machine utilisation
                // productionOrderOperation.Machine,

                // for simulation
                // Start, End,

                // for backward scheduling
                // StartBackward, EndBackward,

                // for forward scheduling
                // StartForward, EndForward,
            }

        }

        public T_ProductionOrderOperation GetValue()
        {
            return _productionOrderOperation;
        }
    }
}