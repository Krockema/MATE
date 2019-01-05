using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Material
    {
        public string MaterialId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public double? LotSizeMax { get; set; }
        public double? LotSizeMin { get; set; }
        public double? LotSizeOpt { get; set; }
        public long? InhouseProduction { get; set; }
        public long? PurchaseTimeQuantityDependent { get; set; }
        public long? PurchaseTimeQuantityIndependent { get; set; }
        public long? WaitingTimeMax { get; set; }
        public string QuantityUnitId { get; set; }
        public double? ValuePurchase { get; set; }
        public double? ValueSales { get; set; }
        public double? ValueProduction { get; set; }
        public double? SafetyStockValue { get; set; }
        public long? SafetyStockUsage { get; set; }
        public long? ReduceStockQuantity { get; set; }
        public long? CheckStockQuantity { get; set; }
        public double? QuantityRoundingValue { get; set; }
    }
}
