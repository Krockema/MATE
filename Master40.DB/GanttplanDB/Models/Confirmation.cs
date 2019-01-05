using System;
using System.Collections.Generic;

namespace Master40.DB.GanttplanDB.Models
{
    public partial class Confirmation
    {
        public string ConfirmationId { get; set; }
        public string Name { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string ProductionorderId { get; set; }
        public string ProductionorderOperationId { get; set; }
        public string ProductionorderAlternativeId { get; set; }
        public long? ProductionorderSplitId { get; set; }
        public long? ProductionorderActivityId { get; set; }
        public string ActivityStart { get; set; }
        public string ActivityEnd { get; set; }
        public double? QuantityFinished { get; set; }
        public string QuantityFinishedUnitId { get; set; }
        public string ConfirmationDate { get; set; }
        public long? ConfirmationType { get; set; }
    }
}
