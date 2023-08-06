using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment.Records.Interfaces;

namespace Mate.Production.Core.Agents.ResourceAgent
{
    public partial class Resource
    {
        public partial class Instruction
        {
            public record Queuing
            {
                public record DoJob : HiveMessage
                {
                    public static DoJob Create(IQueueingJob fQueuingSetup, IActorRef target)
                    {
                        return new DoJob(message: fQueuingSetup, target: target);
                    }
                    private DoJob(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public IQueueingJob GetObjectFromMessage => Message as IQueueingJob; 
                }

                public record FinishJob : HiveMessage
                {
                    public static FinishJob Create(IQueueingJob fQueuingJob,IActorRef target)
                    {
                        return new FinishJob(message: fQueuingJob, target: target);
                    }
                    private FinishJob(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }

                    public IQueueingJob GetObjectFromMessage => Message as IQueueingJob;
                }

            }
        }
    }
}