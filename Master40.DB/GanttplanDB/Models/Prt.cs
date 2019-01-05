using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Prt
    {
        public string PrtId { get; set; }
        public string Name { get; set; }
        public long? CapacityType { get; set; }
        public double? AllocationMax { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public double? FactorProcessingSpeed { get; set; }
        public long? MaintenanceIntervalTime { get; set; }
        public long? MaintenanceIntervalQuantity { get; set; }
        public string MaintenanceIntervalQuantityUnitId { get; set; }
        public long? SetupTime { get; set; }
        public long? AllowChangeWorkcenter { get; set; }
    }
}
