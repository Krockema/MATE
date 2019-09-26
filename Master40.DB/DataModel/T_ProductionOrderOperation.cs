using System.Collections;
using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_ProductionOrderOperation : BaseEntity, IOperation
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int ResourceSkillId { get; set; }
        [JsonIgnore]
        public M_ResourceSkill ResourceSkill { get; set; }
        public int? ResourceId { get; set; }
        [JsonIgnore]
        public M_Resource Resource { get; set; }
        public int? ResourceToolId { get; set; }
        [JsonIgnore]
        public M_ResourceTool ResourceTool { get; set; }
        public int ProductionOrderId { get; set; }
        [JsonIgnore]
        public T_ProductionOrder ProductionOrder { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int? StartBackward { get; set; }
        public int? EndBackward { get; set; }
        public int? StartForward { get; set; }
        public int? EndForward { get; set; }
        public decimal ActivitySlack { get; set; }
        public decimal WorkTimeWithParents { get; set; }
        public int DurationSimulation { get; set; }
        public ProducingState ProducingState { get; set; }
        public ICollection<T_ProductionOrderBom> ProductionOrderBoms { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
        
        public Duration GetDuration()
        {
            return new Duration(Duration);
        }

        public HierarchyNumber GetHierarchyNumber()
        {
            return new HierarchyNumber(HierarchyNumber);
        }
    }
}
