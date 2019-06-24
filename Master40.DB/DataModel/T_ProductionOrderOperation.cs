using System.Collections;
using System.Collections.Generic;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_ProductionOrderOperation : BaseEntity, IOperation, ISimulationProductionOrderWorkSchedule
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int? MachineToolId { get; set; }
        [JsonIgnore]
        public M_MachineTool MachineTool { get; set; }
        public int MachineGroupId { get; set; }
        [JsonIgnore]
        public M_MachineGroup MachineGroup { get; set; }
        public int? MachineId { get; set; }
        [JsonIgnore]
        public M_Machine Machine { get; set; }
        public int? ProductionOrderId { get; set; }
        [JsonIgnore]
        public T_ProductionOrder ProductionOrder { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int StartBackward { get; set; }
        public int EndBackward { get; set; }
        public int StartForward { get; set; }
        public int EndForward { get; set; }
        
        public ProducingState ProducingState { get; set; }
        public ICollection<T_ProductionOrderBom> ProductionOrderBoms { get; set; }
        
        public T_ProductionOrderOperation(){}
        
        public T_ProductionOrderOperation(M_ArticleBom articleBom)
        {
            if (articleBom.ArticleChild.ToBuild)
            {

                T_ProductionOrderOperation productionOrderOperation = new T_ProductionOrderOperation();
                // TODO: add not only entities but also the ids !!! --> only ids should be enough???
                productionOrderOperation.Name = articleBom.Operation.Name;
                productionOrderOperation.HierarchyNumber = articleBom.Operation.HierarchyNumber;
                productionOrderOperation.Duration = articleBom.Operation.Duration;
                productionOrderOperation.MachineTool = articleBom.Operation.MachineTool;
                productionOrderOperation.MachineToolId = articleBom.Operation.MachineToolId;
                productionOrderOperation.MachineGroup = articleBom.Operation.MachineGroup;
                productionOrderOperation.MachineGroupId = articleBom.Operation.MachineGroupId;
                productionOrderOperation.ProducingState = ProducingState.Created;

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

    }
}
