using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblPrtCalendarintervalUdf
    {
        public string ClientId { get; set; }
        public string PrtId { get; set; }
        public DateTime DateFrom { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
