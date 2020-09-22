using System;
using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.DB.GanttPlanModel
{
    public class GptblProductionorderOperationActivityMaterialrelation : GanttKey
    {
        public string ClientId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public GptblProductionorderOperationActivity ProductionorderOperationActivity { get; set; }
        public int MaterialrelationType { get; set; }
        public string ChildId { get; set; }
        public string ChildOperationId { get; set; }
        public string ChildAlternativeId { get; set; }
        public int ChildActivityId { get; set; }
        public int ChildSplitId { get; set; }
        public int? Fixed { get; set; }
        public DateTime? InfoDateAvailability { get; set; }
        public double? InfoTimeBuffer { get; set; }
        public double? OverlapValue { get; set; }
        public double? Quantity { get; set; }
        public string QuantityUnitId { get; set; }
        public string GetChildKey { get => $"{ChildId}|{ChildOperationId}|{ChildActivityId}"; }
    }
}
