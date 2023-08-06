using Akka.Actor;
using Akka.Hive.Definitions;
using static FCentralActivities;

namespace Mate.Production.Core.Agents.ResourceAgent
{
    public partial class Resource
    {
        public partial class Instruction
        {
            public record Central
            {
                public record ActivityStart : HiveMessage
                {
                    public static ActivityStart Create(FCentralActivity activity, IActorRef target)
                    {
                        return new ActivityStart(message: activity, target: target);
                    }
                    private ActivityStart(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralActivity GetObjectFromMessage => Message as FCentralActivity; 
                }
                public record ActivityFinish : HiveMessage
                {
                    public static ActivityFinish Create(IActorRef target)
                    {
                        return new ActivityFinish(message: null, target: target);
                    }
                    private ActivityFinish(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                }

            }
        }
    }
}