using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingOperationActivityResourcereservation
    {
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long SplitId { get; set; }
        public long ActivityId { get; set; }
        public string ReservationId { get; set; }
        public long ReservationType { get; set; }
        public long ResourceType { get; set; }
        public string ResourceId { get; set; }
    }
}
