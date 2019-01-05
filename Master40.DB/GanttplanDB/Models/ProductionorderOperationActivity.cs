using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class ProductionorderOperationActivity
    {
        public string ProductionorderId { get; set; }
        public string OperationId { get; set; }
        public string AlternativeId { get; set; }
        public long ActivityId { get; set; }
        public long SplitId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string InfoNote { get; set; }
        public long? OperationType { get; set; }
        public long? ActivityType { get; set; }
        public long? Status { get; set; }
        public double? QuantityGross { get; set; }
        public double? QuantityNet { get; set; }
        public long? DurationFix { get; set; }
        public double? InfoDuration { get; set; }
        public double? InfoWorkerUtilization { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }
        public double? QuantityFinishedPercentage { get; set; }
        public string LastConfirmationDate { get; set; }
        public string EarliestStartDate { get; set; }
        public string DateStartFix { get; set; }
        public string InfoDateStartInitial { get; set; }
        public string InfoDateEndInitial { get; set; }
        public double? InfoTimeBufferLatestEnd { get; set; }
        public string InfoSetup { get; set; }
        public string InfoDateEarliestStartInitial { get; set; }
        public string InfoDateEarliestEndInitial { get; set; }
        public string InfoDateLatestStartInitial { get; set; }
        public string InfoDateLatestEndInitial { get; set; }
        public string InfoDateEarliestStartMaterial { get; set; }
        public string InfoDateEarliestEndMaterial { get; set; }
        public string InfoDateLatestStartMaterial { get; set; }
        public string InfoDateLatestEndMaterial { get; set; }
        public double? InfoTimeBufferInitial { get; set; }
        public double? InfoTimeBufferMaterial { get; set; }
        public double? InfoTimeBufferScheduling { get; set; }
        public string InfoDebug { get; set; }
        public double? ValueProduction { get; set; }
        public double? ValueProductionAccumulated { get; set; }
        public long? PriorityValue { get; set; }
        public long? SchedulingLevel { get; set; }
        public string InfoDateEarliestStartScheduling { get; set; }
        public string InfoDateEarliestEndScheduling { get; set; }
        public string InfoDateLatestStartScheduling { get; set; }
        public string InfoDateLatestEndScheduling { get; set; }
        public string JobParallelId { get; set; }
        public string JobSequentialId { get; set; }
        public double? InfoValueWorkcenter { get; set; }
        public double? InfoValueWorker { get; set; }
    }
}
