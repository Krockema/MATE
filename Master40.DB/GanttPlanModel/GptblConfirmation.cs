using System;
using System.Security;
using Master40.DB.Data.Helper;
using Master40.DB.Nominal;
using NLog.Common;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblConfirmation
    {
        public string ClientId { get; set; }
        public string ConfirmationId { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public DateTime? ActivityEnd { get; set; }
        public DateTime? ActivityStart { get; set; }
        public int? ConfirmationType { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public int? ProductionorderActivityId { get; set; }
        public string ProductionorderAlternativeId { get; set; }
        public string ProductionorderId { get; set; }
        public string ProductionorderOperationId { get; set; }
        public int? ProductionorderSplitId { get; set; }
        public double? QuantityFinished { get; set; }
        public string QuantityFinishedUnitId { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
