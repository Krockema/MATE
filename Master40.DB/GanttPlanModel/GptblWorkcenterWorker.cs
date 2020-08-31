namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorkcenterWorker
    {
        public string ClientId { get; set; }
        public string WorkcenterId { get; set; }
        public string GroupId { get; set; }
        public string ActivityqualificationId { get; set; }
        public int? ActivityType { get; set; }
        public int? ChangeWorkerType { get; set; }
        public int? WorkerRequirementCount { get; set; }
        public int? WorkerRequirementCountMax { get; set; }
        public double? WorkerRequirementUtilization { get; set; }
    }
}
