using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Interfaces;

namespace Master40.DB.Data.WrappersForPrimitives
{
    public abstract class GanttKey : IGanttKey
    {
        public string ProductionorderId { get; set; }
        public string OperationId { get; set; }
        public int ActivityId { get; set; }
        [NotMapped]
        public string GetKey { get => $"{ProductionorderId}|{OperationId}|{ActivityId}"; }
    }
}