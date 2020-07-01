using AkkaSim;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public interface ICollectorBehaviour
    {
        Collector Collector { get; set; }
        bool EventHandle(SimulationMonitor simulationMonitor, object message);
    }
}