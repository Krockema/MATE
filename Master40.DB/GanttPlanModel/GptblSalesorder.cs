using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblSalesorder
    {
        public string ClientId { get; set; }
        public string SalesorderId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public DateTime? Duedate { get; set; }
        public int? Locked { get; set; }
        public string MaterialId { get; set; }
        public int? PlanningType { get; set; }
        public string PriorityId { get; set; }
        public double? Quantity { get; set; }
        public double? QuantityDelivered { get; set; }
        public string QuantityUnitId { get; set; }
        public int? SalesorderType { get; set; }
        public int? Status { get; set; }
        public double? ValueSales { get; set; }
        public DateTime? LastModified { get; set; }
        public string ProductioncontrollerId { get; set; }
    }
}
