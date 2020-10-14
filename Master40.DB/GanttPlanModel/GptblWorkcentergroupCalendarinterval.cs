using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkcentergroupCalendarinterval
    {
        public string ClientId { get; set; }
        public string WorkcentergroupId { get; set; }
        public DateTime DateFrom { get; set; }
        public string Name { get; set; }
        public DateTime? DateTo { get; set; }
        public int? IntervalType { get; set; }
        public int? RepetitionType { get; set; }
    }
}
