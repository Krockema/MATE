using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkerWorkcenterqualificationUdf
    {
        public string ClientId { get; set; }
        public string WorkerId { get; set; }
        public string WorkcenterId { get; set; }
        public DateTime ValidFrom { get; set; }
        public int WorkcenterqualificationType { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
