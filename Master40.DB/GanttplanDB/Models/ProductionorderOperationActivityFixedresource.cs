using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ProductionorderOperationActivityFixedresource
    {
        public string ProductionorderId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long ActivityId { get; set; }
        public long SplitId { get; set; }
        public string ResourceId { get; set; }
        public long ResourceType { get; set; }
        public string GroupId { get; set; }
    }
}
