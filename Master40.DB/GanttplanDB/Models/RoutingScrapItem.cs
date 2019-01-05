using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class RoutingScrapItem
    {
        public string RoutingId { get; set; }
        public double QuantityLimit { get; set; }
        public double? ScrapRate { get; set; }
    }
}
