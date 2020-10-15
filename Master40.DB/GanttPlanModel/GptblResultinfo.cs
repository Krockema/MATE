using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblResultinfo
    {
        public string ClientId { get; set; }
        public string ResultinfoId { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Name { get; set; }
        public int? CountProductionorderEarly { get; set; }
        public int? CountProductionorderIncomplete { get; set; }
        public int? CountProductionorderLate { get; set; }
        public int? CountProductionorderOntime { get; set; }
        public int? CountSalesorderIncomplete { get; set; }
        public int? OptimizationRun { get; set; }
        public double? ValueCapitalCommitment { get; set; }
        public double? ValueLateness { get; set; }
        public double? ValueObjectiveFunction { get; set; }
        public double? ValueProcessing { get; set; }
        public double? ValueSetup { get; set; }
        public double? ValueThroughputTime { get; set; }
        public double? ValueTotal { get; set; }
        public double? ValueUtilization { get; set; }
        public double? ValueWorker { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
