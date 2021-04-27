namespace Mate.DataCore.ReportingModel
{
    public class SimulationConfig : ResultBaseEntity
    {
        public int SimulationNumber { get; set; }
        public string Property { get; set; }
        public string PropertyValue { get; set; }
    }
}