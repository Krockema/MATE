using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.HubAgent.Buckets.Instructions
{
    public class AddWorkItemToBucket : SimulationMessage
    {
        public static AddWorkItemToBucket Create(FWorkItem message, IActorRef target)
        {
            return new AddWorkItemToBucket(message, target);
        }
        private AddWorkItemToBucket(object message, IActorRef target) : base(message, target)
        {

        }
        public FWorkItem GetObjectFromMessage { get => Message as FWorkItem; }
    }
}
