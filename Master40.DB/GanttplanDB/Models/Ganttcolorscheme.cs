using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Ganttcolorscheme
    {
        public string GanttcolorschemeId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public long? DefaultType { get; set; }
        public long? DefaultColor { get; set; }
        public string DefaultGanttcolorschemeId { get; set; }
        public string DefaultRandomcolorPropertyId { get; set; }
        public string UserId { get; set; }
    }
}
