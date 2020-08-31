using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkerCalendarintervalUdf
    {
        public string ClientId { get; set; }
        public string WorkerId { get; set; }
        public DateTime DateFrom { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
