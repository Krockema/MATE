using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblMaterial
    {
        public string ClientId { get; set; }
        public string MaterialId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public int? CheckStockQuantity { get; set; }
        public int? InhouseProduction { get; set; }
        public double? LotSizeMax { get; set; }
        public double? LotSizeMin { get; set; }
        public double? LotSizeOpt { get; set; }
        public int? PurchaseTimeQuantityDependent { get; set; }
        public int? PurchaseTimeQuantityIndependent { get; set; }
        public double? QuantityRoundingValue { get; set; }
        public string QuantityUnitId { get; set; }
        public int? ReduceStockQuantity { get; set; }
        public int? SafetyStockUsage { get; set; }
        public double? SafetyStockValue { get; set; }
        public double? ValueProduction { get; set; }
        public double? ValuePurchase { get; set; }
        public double? ValueSales { get; set; }
        public int? WaitingTimeMax { get; set; }
        public DateTime? LastModified { get; set; }
        public string ProductioncontrollerId { get; set; }
    }
}
