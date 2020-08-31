namespace Master40.DB.GanttPlanModel
{
    public partial class Material
    {
        public string SessionId { get; set; }
        public string ClientId { get; set; }
        public string ResultId { get; set; }
        public string MaterialId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string LotSizeMax { get; set; }
        public string LotSizeMin { get; set; }
        public string LotSizeOpt { get; set; }
        public string InhouseProduction { get; set; }
        public string PurchaseTimeQuantityDependent { get; set; }
        public string PurchaseTimeQuantityIndependent { get; set; }
        public string WaitingTimeMax { get; set; }
        public string QuantityUnitId { get; set; }
        public string ValuePurchase { get; set; }
        public string ValueSales { get; set; }
        public string ValueProduction { get; set; }
        public string SafetyStockValue { get; set; }
        public string SafetyStockUsage { get; set; }
        public string ReduceStockQuantity { get; set; }
        public string CheckStockQuantity { get; set; }
        public string QuantityRoundingValue { get; set; }
        public string LastModified { get; set; }
        public string ProductioncontrollerId { get; set; }
    }
}
