using AkkaSim;

namespace Master40.SimulationCore.Agents.CollectorAgent
{
    public interface ICollectorBehaviour
    {
        bool EventHandle(SimulationMonitor simulationMonitor, object message);
    }
}