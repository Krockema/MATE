using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Salesorder
    {
        public string SalesorderId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Duedate { get; set; }
        public string MaterialId { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public string PriorityId { get; set; }
        public long? SalesorderType { get; set; }
        public long? Status { get; set; }
        public long? PlanningType { get; set; }
        public double? ValueSales { get; set; }
        public long? Locked { get; set; }
        public double? QuantityDelivered { get; set; }
    }
}
