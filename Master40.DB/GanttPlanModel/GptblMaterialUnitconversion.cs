namespace Master40.DB.GanttPlanModel
{
    public partial class GptblMaterialUnitconversion
    {
        public string ClientId { get; set; }
        public string MaterialId { get; set; }
        public string UnitId { get; set; }
        public string ConversionUnitId { get; set; }
        public double? ConversionFactor { get; set; }
    }
}
