using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkergroupActivityqualificationUdf
    {
        public string ClientId { get; set; }
        public string WorkergroupId { get; set; }
        public string ActivityqualificationId { get; set; }
        public DateTime ValidFrom { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
