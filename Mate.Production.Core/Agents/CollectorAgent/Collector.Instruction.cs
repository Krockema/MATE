using Akka.Actor;
using Akka.Hive.Definitions;

namespace Mate.Production.Core.Agents.CollectorAgent
{
    public partial class Collector
    {
        public record Instruction
        {
            public record UpdateLiveFeed : HiveMessage
            {
                public static UpdateLiveFeed Create(bool setup, IActorRef target)
                {
                    return new UpdateLiveFeed(message: setup, target: target);
                }
                private UpdateLiveFeed(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public bool GetObjectFromMessage { get => (bool)this.Message; }
            }
        }
    }
}
