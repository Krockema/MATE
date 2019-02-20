using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents
{
    public partial class Collector
    {
        public class Instruction
        {
            public class UpdateLiveFeed : SimulationMessage
            {
                public static UpdateLiveFeed Create(bool setup, IActorRef target)
                {
                    return new UpdateLiveFeed(setup, target);
                }
                private UpdateLiveFeed(object message, IActorRef target) : base(message, target)
                {
                }
                public bool GetObjectFromMessage { get => (bool)this.Message; }
            }
        }
    }
}
