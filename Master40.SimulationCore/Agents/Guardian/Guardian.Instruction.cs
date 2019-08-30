using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationCore.Helper;

namespace Master40.SimulationCore.Agents.Guardian
{
    public class Instruction
    {
        public class CreateChild : SimulationMessage
        {
            public IActorRef Source  { get; }
            public static CreateChild Create(AgentSetup setup, IActorRef target, IActorRef source)
            {
                return new CreateChild(message: setup, target: target, source: source);
            }
            private CreateChild(object message, IActorRef target, IActorRef source) : base(message: message, target: target)
            {
                Source = source;
            }
            public AgentSetup GetObjectFromMessage { get => this.Message as AgentSetup; }
        }
    }
}
