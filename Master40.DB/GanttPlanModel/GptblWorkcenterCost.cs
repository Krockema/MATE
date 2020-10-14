using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkcenterCost
    {
        public string ClientId { get; set; }
        public string WorkcenterId { get; set; }
        public DateTime ValidFrom { get; set; }
        public double? CostRateIdleTime { get; set; }
        public double? CostRateProcessing { get; set; }
        public double? CostRateSetup { get; set; }
    }
}
