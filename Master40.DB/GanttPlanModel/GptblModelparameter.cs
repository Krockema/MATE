using System;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblModelparameter
    {
        public string ClientId { get; set; }
        public string ModelparameterId { get; set; }
        public DateTime? ActualTime { get; set; }
        public int? ActualTimeFromSystemTime { get; set; }
        public int? AllowChangeWorkerActivityTimeMin { get; set; }
        public int? AllowMultipleMachineWork { get; set; }
        public int? AllowOverlapActivityTypeSetup { get; set; }
        public int? AllowOverlapActivityTypeWait { get; set; }
        public int? AutoCalculatePeriods { get; set; }
        public int? AutoConfirmChildProductionorders { get; set; }
        public DateTime? CapacityPeriodEnd { get; set; }
        public int? CapacityPeriodPast { get; set; }
        public int? CapacityPeriodPlanning { get; set; }
        public DateTime? CapacityPeriodStart { get; set; }
        public double? CapitalCommitmentInterestRate { get; set; }
        public DateTime? DataPeriodEnd { get; set; }
        public int? DataPeriodPlanning { get; set; }
        public int? ObjectiveFunctionType { get; set; }
        public int? SchedulePrt { get; set; }
        public int? ScheduleWorker { get; set; }
        public int? SchedulingStatusLate { get; set; }
        public int? SchedulingStatusOntime { get; set; }
        public string StockPriorityId { get; set; }
        public int? TimeBufferPurchaseorder { get; set; }
        public int? TimeBufferSalesorder { get; set; }
        public int? WorkerIntervalTimeMin { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
