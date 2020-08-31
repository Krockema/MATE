using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkerWorkcenterqualification
    {
        public string ClientId { get; set; }
        public string WorkerId { get; set; }
        public string WorkcenterId { get; set; }
        public DateTime ValidFrom { get; set; }
        public int WorkcenterqualificationType { get; set; }
        public int? PriorityValue { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
