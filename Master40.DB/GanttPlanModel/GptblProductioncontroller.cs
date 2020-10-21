using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblProductioncontroller
    {
        public string ClientId { get; set; }
        public string ProductioncontrollerId { get; set; }
        public string Name { get; set; }
        public string PlantId { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
