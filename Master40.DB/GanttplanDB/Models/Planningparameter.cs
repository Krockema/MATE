using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Planningparameter
    {
        public string PlanningparameterId { get; set; }
        public long? StablePeriod { get; set; }
        public double? ImportanceSetup { get; set; }
        public double? ImportanceWorker { get; set; }
        public double? ImportanceProcessing { get; set; }
        public double? ImportanceCapitalCommitment { get; set; }
        public double? ImportanceThroughputTime { get; set; }
        public double? ImportanceLateness { get; set; }
        public double? ImportanceUtilization { get; set; }
        public long? StrategyType { get; set; }
        public long? OptimizationRunCount { get; set; }
        public long? ResultCount { get; set; }
        public long? PlanningTypes { get; set; }
        public long? PlanningMode { get; set; }
        public long? MrpCheckInhouseProduction { get; set; }
        public long? MrpCheckPurchase { get; set; }
        public long? MrpCreateInhouseProduction { get; set; }
        public long? MrpCreatePurchase { get; set; }
        public long? MrpRelinkProductionorders { get; set; }
        public long? MrpRelinkPurchaseorders { get; set; }
        public long? MrpRelinkStockreservations { get; set; }
        public long? LotsizeOptimization { get; set; }
    }
}
