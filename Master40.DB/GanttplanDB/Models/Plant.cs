using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Plant
    {
        public string PlantId { get; set; }
        public string Name { get; set; }
        public long? Default { get; set; }
    }
}
