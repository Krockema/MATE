using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkergroupPoolcapacity
    {
        public string ClientId { get; set; }
        public string WorkergroupId { get; set; }
        public DateTime DateFrom { get; set; }
        public int? Quantity { get; set; }
    }
}
