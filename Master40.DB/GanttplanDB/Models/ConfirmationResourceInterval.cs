using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ConfirmationResourceInterval
    {
        public string ConfirmationId { get; set; }
        public string ResourceId { get; set; }
        public long ResourceType { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public long? IntervalAllocationType { get; set; }
    }
}
