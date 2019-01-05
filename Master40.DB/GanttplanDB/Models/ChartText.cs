using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ChartText
    {
        public string ChartId { get; set; }
        public string TextId { get; set; }
        public long? TextType { get; set; }
    }
}
