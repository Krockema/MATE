using Akka.Actor;
using Akka.Hive.Definitions;
using static FProposals;
using static FResourceInformations;
using static IJobs;

namespace Mate.Production.Core.Agents.HubAgent
{
    public partial class Hub
    {
        /// <summary>
        /// Add instructions for new behaviour in separate class
        /// </summary>
        public partial class Instruction
        {

            public record Default
            {
                public record AddResourceToHub : HiveMessage
                {
                    public static AddResourceToHub Create(FResourceInformation message, IActorRef target, bool logThis = false)
                    {
                        return new AddResourceToHub(message: message, target: target, logThis: logThis);
                    }
                    private AddResourceToHub(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
                    {

                    }
                    public FResourceInformation GetObjectFromMessage { get => Message as FResourceInformation; }
                }

                public record EnqueueJob : HiveMessage
                {
                    public static EnqueueJob Create(IJob message, IActorRef target)
                    {
                        return new EnqueueJob(message: message, target: target);
                    }
                    private EnqueueJob(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public IJob GetObjectFromMessage { get => Message as IJob; }
                }


                public record ProposalFromResource : HiveMessage
                {
                    public static ProposalFromResource Create(FProposal message, IActorRef target)
                    {
                        return new ProposalFromResource(message: message, target: target);
                    }
                    private ProposalFromResource(object message, IActorRef target) : base(message: message, target: target)
                    {

                    }
                    public FProposal GetObjectFromMessage { get => Message as FProposal; }
                }


            }
        }
    }
}