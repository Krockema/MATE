using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkergroupActivityqualification
    {
        public string ClientId { get; set; }
        public string WorkergroupId { get; set; }
        public string ActivityqualificationId { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? PriorityValue { get; set; }
    }
}
