using System;
using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Data.Helper;

namespace Master40.DB.GanttPlanModel
{
    public partial class GptblProductionorderOperationActivityResourceInterval
    {
        public string ClientId { get; set; }
        public string ProductionorderId { get; set; }
        public string OperationId { get; set; }
        public int ActivityId { get; set; }
        public string AlternativeId { get; set; }
        public int SplitId { get; set; }
        public GptblProductionorderOperationActivityResource ProductionorderOperationActivityResource
        {
            get;
            set;
        }
        public DateTime DateFrom { get; set; }
        [NotMapped] public long ConvertedDateFrom => DateFrom.ToSimulationTime();
        public string ResourceId { get; set; }
        public int ResourceType { get; set; }
        public DateTime DateTo { get; set; }
        [NotMapped] public long ConvertedDateTo => DateTo.ToSimulationTime();
        public int? IntervalAllocationType { get; set; }
    }
}
