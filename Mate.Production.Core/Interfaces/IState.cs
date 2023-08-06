using Akka.Hive;

namespace Mate.Production.Core.Interfaces
{
    public interface IStateManager
    {
        void AfterSimulationStarted();
        void AfterSimulationStopped();
        void SimulationIsTerminating();
    }
}