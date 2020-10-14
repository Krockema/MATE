using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkerActivityqualification
    {
        public string ClientId { get; set; }
        public string WorkerId { get; set; }
        public string ActivityqualificationId { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? PriorityValue { get; set; }
    }
}
