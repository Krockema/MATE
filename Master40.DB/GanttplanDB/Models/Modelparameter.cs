using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Modelparameter
    {
        public string ModelparameterId { get; set; }
        public long? ScheduleWorker { get; set; }
        public long? SchedulePrt { get; set; }
        public long? AllowMultipleMachineWork { get; set; }
        public long? AllowOverlapActivityTypeSetup { get; set; }
        public long? AllowOverlapActivityTypeWait { get; set; }
        public long? AllowChangeWorkerActivityTimeMin { get; set; }
        public long? WorkerIntervalTimeMin { get; set; }
        public long? SchedulingStatusOntime { get; set; }
        public long? SchedulingStatusLate { get; set; }
        public double? CapitalCommitmentInterestRate { get; set; }
        public long? ObjectiveFunctionType { get; set; }
        public long? CapacityPeriodPast { get; set; }
        public long? CapacityPeriodPlanning { get; set; }
        public long? DataPeriodPlanning { get; set; }
        public long? TimeBufferSalesorder { get; set; }
        public long? TimeBufferPurchaseorder { get; set; }
        public long? AutoConfirmChildProductionorders { get; set; }
        public string ActualTime { get; set; }
        public long? ActualTimeFromSystemTime { get; set; }
        public string DataPeriodEnd { get; set; }
        public string CapacityPeriodStart { get; set; }
        public string CapacityPeriodEnd { get; set; }
        public long? AutoCalculatePeriods { get; set; }
        public string StockPriorityId { get; set; }
    }
}
