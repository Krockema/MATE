using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRouting
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public int? AllowOverlap { get; set; }
        public int? Locked { get; set; }
        public string MasterRoutingId { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
