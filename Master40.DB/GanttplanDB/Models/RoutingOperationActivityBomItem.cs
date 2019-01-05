using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingOperationActivityBomItem
    {
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public long SplitId { get; set; }
        public long ActivityId { get; set; }
        public string BomItemId { get; set; }
    }
}
