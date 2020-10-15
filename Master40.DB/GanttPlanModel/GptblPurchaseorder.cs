using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblPurchaseorder
    {
        public string ClientId { get; set; }
        public string PurchaseorderId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? InfoOrderDate { get; set; }
        public int? Locked { get; set; }
        public string MaterialId { get; set; }
        public int? PurchaseorderType { get; set; }
        public double? Quantity { get; set; }
        public double? QuantityDelivered { get; set; }
        public string QuantityUnitId { get; set; }
        public int? Status { get; set; }
        public DateTime? LastModified { get; set; }
        public string ProductioncontrollerId { get; set; }
    }
}
