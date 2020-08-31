using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblConfirmationResourceIntervalUdf
    {
        public string ClientId { get; set; }
        public int ResourceType { get; set; }
        public string ConfirmationId { get; set; }
        public string ResourceId { get; set; }
        public DateTime DateFrom { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
