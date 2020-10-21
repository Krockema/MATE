using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblOptimizationgroup
    {
        public string ClientId { get; set; }
        public string OptimizationgroupId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public double? ValueDifferenceMax { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
