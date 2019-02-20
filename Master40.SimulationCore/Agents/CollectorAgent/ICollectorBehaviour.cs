using AkkaSim;

namespace Master40.SimulationCore.Agents
{
    public interface ICollectorBehaviour
    {
        bool EventHandle(SimulationMonitor simulationMonitor, object message);
    }
}