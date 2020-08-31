using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblProductionorder
    {
        public string ClientId { get; set; }
        public string ProductionorderId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public int? Approved { get; set; }
        public DateTime? DateEnd { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? Duedate { get; set; }
        public DateTime? EarliestStartDate { get; set; }
        public int? FixedProperties { get; set; }
        public DateTime? InfoDateEndInitial { get; set; }
        public DateTime? InfoDateStartInitial { get; set; }
        public string InfoDebug { get; set; }
        public int? Locked { get; set; }
        public string MaterialId { get; set; }
        public int? PlanningType { get; set; }
        public string PriorityId { get; set; }
        public double? QuantityGross { get; set; }
        public double? QuantityNet { get; set; }
        public string QuantityUnitId { get; set; }
        public string RoutingId { get; set; }
        public int? SchedulingStatus { get; set; }
        public int? Status { get; set; }
        public double? ValueProduction { get; set; }
        public double? ValueSales { get; set; }
        public DateTime? LastModified { get; set; }
        public string ProductionversionId { get; set; }
        public string LotParentId { get; set; }
        public string BomId { get; set; }
        public string ProductioncontrollerId { get; set; }
    }
}
