namespace Mate.Production.Core.Agents.CollectorAgent.Types
{
    public class ResourceSimulationData
    {
        internal string _resource { get; set; }
        internal string _setupTime { get; set; }
        internal string _workTime { get; set; }
        internal long _totalSetupTime { get; set; }
        internal long _totalWorkTime { get; set; }
        public ResourceSimulationData(string resource)
        {
            _resource = resource;
        }
    }
}
