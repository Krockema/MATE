namespace Master40.DB.GanttPlanModel
{
    public partial class GptblRoutingOperationWorker
    {
        public string ClientId { get; set; }
        public string RoutingId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string GroupId { get; set; }
        public string ActivityqualificationId { get; set; }
        public int? ActivityType { get; set; }
        public int? ChangeWorkerType { get; set; }
        public string WorkcenterId { get; set; }
        public int? WorkerRequirementCount { get; set; }
        public int? WorkerRequirementCountMax { get; set; }
        public double? WorkerRequirementUtilization { get; set; }
    }
}
