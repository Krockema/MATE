namespace Master40.DB.GanttPlanModel
{
    public partial class GptblBomItem
    {
        public string ClientId { get; set; }
        public string BomId { get; set; }
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string AlternativeId { get; set; }
        public string Group { get; set; }
        public string MaterialId { get; set; }
        public int? PreparationTime { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public int? Standard { get; set; }
    }
}
