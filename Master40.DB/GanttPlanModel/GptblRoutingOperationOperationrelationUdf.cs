namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperationOperationrelationUdf
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string SuccessorOperationId { get; set; }
        public string SuccessorAlternativeId { get; set; }
        public int SuccessorSplitId { get; set; }
        public string UdfId { get; set; }
        public string Value { get; set; }
    }
}
