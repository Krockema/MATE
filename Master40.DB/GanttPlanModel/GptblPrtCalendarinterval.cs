using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblPrtCalendarinterval
    {
        public string ClientId { get; set; }
        public string PrtId { get; set; }
        public DateTime DateFrom { get; set; }
        public string Name { get; set; }
        public DateTime? DateTo { get; set; }
        public int? IntervalType { get; set; }
        public int? RepetitionType { get; set; }
    }
}
