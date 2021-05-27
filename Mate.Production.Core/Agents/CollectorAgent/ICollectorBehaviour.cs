using AkkaSim;

namespace Mate.Production.Core.Agents.CollectorAgent
{
    public interface ICollectorBehaviour
    {
        Collector Collector { get; set; }
        bool EventHandle(SimulationMonitor simulationMonitor, object message);
    }
}