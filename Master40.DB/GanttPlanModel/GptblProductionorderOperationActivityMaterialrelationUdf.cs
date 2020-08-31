namespace Master40.DB.GanttPlanModel
{
    public partial class GptblProductionorderOperationActivityMaterialrelationUdf
    {
        public string ClientId { get; set; }
        public string ProductionorderId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public int ActivityId { get; set; }
        public int SplitId { get; set; }
        public int MaterialrelationType { get; set; }
        public string ChildId { get; set; }
        public string ChildOperationId { get; set; }
        public string ChildAlternativeId { get; set; }
        public int ChildActivityId { get; set; }
        public int ChildSplitId { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
