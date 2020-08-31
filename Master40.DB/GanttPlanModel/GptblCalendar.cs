using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblCalendar
    {
        public string ClientId { get; set; }
        public string CalendarId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
