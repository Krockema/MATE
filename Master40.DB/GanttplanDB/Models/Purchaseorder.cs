using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Purchaseorder
    {
        public string PurchaseorderId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string DeliveryDate { get; set; }
        public string MaterialId { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public long? PurchaseorderType { get; set; }
        public long? Locked { get; set; }
        public long? Status { get; set; }
        public double? QuantityDelivered { get; set; }
        public string InfoOrderDate { get; set; }
    }
}
