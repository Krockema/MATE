using System;
using System.Collections.Generic;
using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Interfaces;
using Mate.DataCore.Nominal;
using Newtonsoft.Json;

namespace Mate.DataCore.DataModel
{
    public class T_ProductionOrderOperation : BaseEntity, IOperation
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public int ResourceCapabilityId { get; set; }
        [JsonIgnore]
        public M_ResourceCapability ResourceCapability { get; set; }
        public int? ResourceId { get; set; }
        [JsonIgnore]
        public M_Resource Resource { get; set; }
        public int? ResourceToolId { get; set; }
        [JsonIgnore]
        public int ProductionOrderId { get; set; }
        [JsonIgnore]
        public T_ProductionOrder ProductionOrder { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime? StartBackward { get; set; }
        public DateTime? EndBackward { get; set; }
        public DateTime? StartForward { get; set; }
        public DateTime? EndForward { get; set; }
        public decimal ActivitySlack { get; set; }
        public decimal WorkTimeWithParents { get; set; }
        public TimeSpan DurationSimulation { get; set; }
        public ProducingState ProducingState { get; set; }
        public State State { get; set; }
        public ICollection<T_ProductionOrderBom> ProductionOrderBoms { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
        
        public HierarchyNumber GetHierarchyNumber()
        {
            return new HierarchyNumber(HierarchyNumber);
        }
    }
}
