using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblShiftIntervalUdf
    {
        public string ClientId { get; set; }
        public string ShiftId { get; set; }
        public DateTime DateFrom { get; set; }
        public byte WeekdayType { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
