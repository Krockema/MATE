using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblProductionorderOperationActivity
    {
        public string ClientId { get; set; }
        public string ProductionorderId { get; set; }
        public string OperationId { get; set; }
        public int ActivityId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public int? ActivityType { get; set; }
        public double? ValueProduction { get; set; }
        public DateTime? DateEnd { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateStartFix { get; set; }
        public DateTime? EarliestStartDate { get; set; }
        public DateTime? InfoDateEarliestEndInitial { get; set; }
        public DateTime? InfoDateEarliestEndMaterial { get; set; }
        public DateTime? InfoDateEarliestEndScheduling { get; set; }
        public DateTime? InfoDateEarliestStartInitial { get; set; }
        public DateTime? InfoDateEarliestStartMaterial { get; set; }
        public DateTime? InfoDateEarliestStartScheduling { get; set; }
        public DateTime? InfoDateEndInitial { get; set; }
        public DateTime? InfoDateLatestEndInitial { get; set; }
        public DateTime? InfoDateLatestEndMaterial { get; set; }
        public DateTime? InfoDateLatestEndScheduling { get; set; }
        public DateTime? InfoDateLatestStartInitial { get; set; }
        public DateTime? InfoDateLatestStartMaterial { get; set; }
        public DateTime? InfoDateLatestStartScheduling { get; set; }
        public DateTime? InfoDateStartInitial { get; set; }
        public string InfoDebug { get; set; }
        public double? InfoDuration { get; set; }
        public string InfoNote { get; set; }
        public string InfoSetup { get; set; }
        public double? InfoTimeBufferInitial { get; set; }
        public double? InfoTimeBufferLatestEnd { get; set; }
        public double? InfoTimeBufferMaterial { get; set; }
        public double? InfoTimeBufferScheduling { get; set; }
        public double? InfoWorkerUtilization { get; set; }
        public string JobParallelId { get; set; }
        public string JobSequentialId { get; set; }
        public DateTime? LastConfirmationDate { get; set; }
        public int? OperationType { get; set; }
        public int? PriorityValue { get; set; }
        public double? QuantityFinishedNet { get; set; }
        public double? QuantityGross { get; set; }
        public double? QuantityNet { get; set; }
        public int? SchedulingLevel { get; set; }
        public int? Status { get; set; }
        public double? ValueProductionAccumulated { get; set; }
        public int? DurationFix { get; set; }
        public int? ConfirmationType { get; set; }
    }
}
