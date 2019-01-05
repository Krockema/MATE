using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class WorkingtimemodelShiftmodel
    {
        public string WorkingtimemodelId { get; set; }
        public string StartDate { get; set; }
        public string ShiftmodelId { get; set; }
        public string ValidUntil { get; set; }
        public long? StartShiftIndex { get; set; }
    }
}
