using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Routing
    {
        public string RoutingId { get; set; }
        public string Name { get; set; }
        public long? Locked { get; set; }
        public long? AllowOverlap { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string BomId { get; set; }
        public string MasterRoutingId { get; set; }
    }
}
