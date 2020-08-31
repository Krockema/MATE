using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkerActivityqualificationUdf
    {
        public string ClientId { get; set; }
        public string WorkerId { get; set; }
        public string ActivityqualificationId { get; set; }
        public DateTime ValidFrom { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
