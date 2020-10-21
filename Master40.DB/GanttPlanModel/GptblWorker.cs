using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Interfaces;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblWorker : IGptblResource
    {
        public string ClientId { get; set; }
        public string WorkerId { get; set; }
        [NotMapped]
        public string Id { get => WorkerId; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Name { get; set; }
        public double? AllocationMax { get; set; }
        public int? CapacityType { get; set; }
        public double? CostRate { get; set; }
        public double? ProcessingTimePenalty { get; set; }
        public double? SetupTimePenalty { get; set; }
        public DateTime? LastModified { get; set; }
        public string GlobalCalendarId { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}
