using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Resultinfo
    {
        public string ResultinfoId { get; set; }
        public string Name { get; set; }
        public long? OptimizationRun { get; set; }
        public long? CountProductionorderEarly { get; set; }
        public long? CountProductionorderOntime { get; set; }
        public long? CountProductionorderLate { get; set; }
        public long? CountProductionorderIncomplete { get; set; }
        public long? CountSalesorderIncomplete { get; set; }
        public double? ValueSetup { get; set; }
        public double? ValueWorker { get; set; }
        public double? ValueProcessing { get; set; }
        public double? ValueThroughputTime { get; set; }
        public double? ValueCapitalCommitment { get; set; }
        public double? ValueLateness { get; set; }
        public double? ValueUtilization { get; set; }
        public double? ValueObjectiveFunction { get; set; }
        public double? ValueTotal { get; set; }
        public string Timestamp { get; set; }
    }
}
