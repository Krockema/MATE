using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Workingtimemodel
    {
        public string WorkingtimemodelId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string IndividualCalendarId { get; set; }
        public string GlobalCalendarId { get; set; }
        public string ValidUntil { get; set; }
    }
}
