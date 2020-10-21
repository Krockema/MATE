using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblShiftInterval
    {
        public string ClientId { get; set; }
        public string ShiftId { get; set; }
        public string Name { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int IntervalType { get; set; }
        public byte WeekdayType { get; set; }
    }
}
