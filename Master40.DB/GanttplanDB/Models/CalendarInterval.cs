using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class CalendarInterval
    {
        public string CalendarId { get; set; }
        public string DateFrom { get; set; }
        public string Name { get; set; }
        public string DateTo { get; set; }
        public long? RepetitionType { get; set; }
        public long? IntervalType { get; set; }
    }
}
