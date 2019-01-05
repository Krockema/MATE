using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class SetupmatrixItemUdf
    {
        public string SetupmatrixId { get; set; }
        public string FromOptimizationgroupId { get; set; }
        public string ToOptimizationgroupId { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
