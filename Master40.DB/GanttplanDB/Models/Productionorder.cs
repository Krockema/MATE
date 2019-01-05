using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Productionorder
    {
        public string ProductionorderId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Duedate { get; set; }
        public string EarliestStartDate { get; set; }
        public string PriorityId { get; set; }
        public long? Approved { get; set; }
        public long? Locked { get; set; }
        public string MaterialId { get; set; }
        public string RoutingId { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }
        public string InfoDateStartInitial { get; set; }
        public string InfoDateEndInitial { get; set; }
        public long? SchedulingStatus { get; set; }
        public long? Status { get; set; }
        public string InfoDebug { get; set; }
        public string QuantityUnitId { get; set; }
        public double? QuantityNet { get; set; }
        public double? QuantityGross { get; set; }
        public double? ValueProduction { get; set; }
        public double? ValueSales { get; set; }
        public long? FixedProperties { get; set; }
        public long? PlanningType { get; set; }
    }
}
