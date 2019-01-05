using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Resourcestatus
    {
        public string ResourceId { get; set; }
        public long ResourceType { get; set; }
        public string Name { get; set; }
        public long? BgRed { get; set; }
        public long? BgGreen { get; set; }
        public long? BgBlue { get; set; }
        public string Date { get; set; }
        public long? UsedTime { get; set; }
        public long? UsedQuantity { get; set; }
        public string UsedQuantityUnit { get; set; }
    }
}
