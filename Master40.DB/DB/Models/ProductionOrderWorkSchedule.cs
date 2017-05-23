using Master40.DB.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Master40.DB.Models
{
    public class ProductionOrderWorkSchedule : BaseEntity, IWorkSchedule
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int? MachineToolId { get; set; }
        public MachineTool MachineTool { get; set; }
        public int MachineGroupId { get; set; }
        public MachineGroup MachineGroup { get; set; }
        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int StartBackward { get; set; }
        public int EndBackward { get; set; }
        public int StartForward { get; set; }
        public int EndForward { get; set; }
        public int ActivitySlack { get; set; }
        public int WorkTimeWithParents { get; set; }
    }
}
