namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperationOptimizationgroup
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public int OptimizationgroupType { get; set; }
        public string OptimizationgroupId { get; set; }
        public double? OptimizationgroupValue { get; set; }
    }
}
