using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ProductionorderOperationActivityMaterialrelation
    {
        public string ProductionorderId { get; set; }
        public string OperationId { get; set; }
        public long ActivityId { get; set; }
        public long SplitId { get; set; }
        public string ChildId { get; set; }
        public string ChildOperationId { get; set; }
        public long ChildActivityId { get; set; }
        public long ChildSplitId { get; set; }
        public long MaterialrelationType { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public double? OverlapValue { get; set; }
        public string InfoDateAvailability { get; set; }
        public double? InfoTimeBuffer { get; set; }
        public long? Fixed { get; set; }
    }
}
