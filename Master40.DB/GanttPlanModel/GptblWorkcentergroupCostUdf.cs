using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkcentergroupCostUdf
    {
        public string ClientId { get; set; }
        public string WorkcentergroupId { get; set; }
        public DateTime ValidFrom { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
