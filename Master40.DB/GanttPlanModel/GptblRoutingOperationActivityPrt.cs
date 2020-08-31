namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperationActivityPrt
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public int ActivityId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string PrtId { get; set; }
        public string GroupId { get; set; }
        public double? PrtAllocation { get; set; }
    }
}
