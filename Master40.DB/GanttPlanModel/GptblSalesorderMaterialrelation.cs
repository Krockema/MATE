using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblSalesorderMaterialrelation
    {
        public string ClientId { get; set; }
        public string SalesorderId { get; set; }
        public string ChildId { get; set; }
        public int MaterialrelationType { get; set; }
        public int? Fixed { get; set; }
        public DateTime? InfoDateAvailability { get; set; }
        public double? InfoTimeBuffer { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
    }
}
