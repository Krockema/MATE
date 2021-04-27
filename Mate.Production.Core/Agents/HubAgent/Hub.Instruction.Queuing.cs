using Akka.Actor;
using AkkaSim.Definitions;
using static IQueueingJobs;

namespace Mate.Production.Core.Agents.HubAgent
{
    public partial class Hub
    {
        /// <summary>
        /// Add instructions for new behaviour in separate class
        /// </summary>
        public partial class Instruction
        {

            public class Queuing
            {

                public class FinishSetup : SimulationMessage
                {
                    public static FinishSetup Create(IQueueingJob fQueuingSetup, IActorRef target)
                    {
                        return new FinishSetup(message: fQueuingSetup, target: target);
                    }
                    private FinishSetup(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public IQueueingJob GetObjectFromMessage => Message as IQueueingJob;
                }
                public class FinishWork : SimulationMessage
                {
                    public static FinishWork Create(IQueueingJob fQueuingJob, IActorRef target)
                    {
                        return new FinishWork(message: fQueuingJob, target: target);
                    }
                    private FinishWork(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public IQueueingJob GetObjectFromMessage => Message as IQueueingJob;
                }

            }
        }
    }
}