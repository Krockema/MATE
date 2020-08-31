using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkergroupPoolcapacityUdf
    {
        public string ClientId { get; set; }
        public string WorkergroupId { get; set; }
        public DateTime DateFrom { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
