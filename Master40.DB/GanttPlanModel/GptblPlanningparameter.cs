using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblPlanningparameter
    {
        public string ClientId { get; set; }
        public string PlanningparameterId { get; set; }
        public double? ImportanceCapitalCommitment { get; set; }
        public double? ImportanceLateness { get; set; }
        public double? ImportanceProcessing { get; set; }
        public double? ImportanceSetup { get; set; }
        public double? ImportanceThroughputTime { get; set; }
        public double? ImportanceUtilization { get; set; }
        public double? ImportanceWorker { get; set; }
        public int? OptimizationRunCount { get; set; }
        public int? PlanningMode { get; set; }
        public int? PlanningTypes { get; set; }
        public int? ResultCount { get; set; }
        public int? StablePeriod { get; set; }
        public int? StrategyType { get; set; }
        public int? MrpCheckInhouseProduction { get; set; }
        public int? MrpCheckPurchase { get; set; }
        public int? MrpCreateInhouseProduction { get; set; }
        public int? MrpCreatePurchase { get; set; }
        public int? MrpRelinkProductionorders { get; set; }
        public int? MrpRelinkPurchaseorders { get; set; }
        public int? MrpRelinkStockreservations { get; set; }
        public int? LotsizeOptimization { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
