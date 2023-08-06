using Akka.Hive.Actors;

namespace Mate.Production.Core.Agents.CollectorAgent
{
    public interface ICollectorBehaviour
    {
        Collector Collector { get; set; }
        bool EventHandle(MessageMonitor simulationMonitor, object message);
    }
}