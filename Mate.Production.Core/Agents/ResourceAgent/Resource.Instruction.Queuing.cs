using Akka.Actor;
using AkkaSim.Definitions;
using static IQueueingJobs;

namespace Mate.Production.Core.Agents.ResourceAgent
{
    public partial class Resource
    {
        public partial class Instruction
        {
            public class Queuing
            {
                public class DoJob : SimulationMessage
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

                public class FinishJob : SimulationMessage
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