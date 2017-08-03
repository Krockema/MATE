namespace Master40.DB.Models
{
    public class SimulationWorkschedule : BaseEntity
    {
        public int SimulationId { get; set; }
        public int Time { get; set; }

        public string Article { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int EstimatedStart { get; set; }
        public int EstimatedEnd { get; set; }
        public string Machine { get; set; }
        public int WorkScheduleId { get; set; }
        public int ProductionOrderId { get; set; }
        public int DueTime { get; set; }
        public int OrderId { get; set; }

        public string Parent { get; set; }
        public string ParentId { get; set; }


    }
}