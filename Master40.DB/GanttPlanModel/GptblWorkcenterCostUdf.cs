using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkcenterCostUdf
    {
        public string ClientId { get; set; }
        public string WorkcenterId { get; set; }
        public DateTime ValidFrom { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
