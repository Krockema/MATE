using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblConfirmationResourceInterval
    {
        public string ClientId { get; set; }
        public string ConfirmationId { get; set; }
        public string ResourceId { get; set; }
        public int ResourceType { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? IntervalAllocationType { get; set; }
    }
}
