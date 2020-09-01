namespace Master40.DB.GanttPlanModel
{
    public partial class GptblProductionorderOperationActivityResource
    {
        public string ClientId { get; set; }
        public string ProductionorderId { get; set; }
        public string OperationId { get; set; }
        public int ActivityId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string ResourceId { get; set; }
        public GptblProductionorderOperationActivity ProductionorderOperationActivity { get; set; }
        public GptblProductionorderOperationActivityResourceInterval ProductionorderOperationActivityResourceInterval
        {
            get;
            set;
        }
        public int ResourceType { get; set; }
        public string GroupId { get; set; }
        public int? Allocation { get; set; }
    }
}
