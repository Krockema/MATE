using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ConfirmationResource
    {
        public string ConfirmationId { get; set; }
        public string ResourceId { get; set; }
        public long ResourceType { get; set; }
        public string GroupId { get; set; }
        public long? Allocation { get; set; }
    }
}
