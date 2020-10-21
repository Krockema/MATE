using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblMaterialProductionversion
    {
        public string ClientId { get; set; }
        public string MaterialId { get; set; }
        public string ProductionversionId { get; set; }
        public string BomId { get; set; }
        public string RoutingId { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
