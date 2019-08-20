using Akka.Actor;
using AkkaSim.Definitions;
using System;
using static FAgentInformations;
using static FProposals;
using static IJobResults;
using static IJobs;

namespace Master40.SimulationCore.Agents.HubAgent
{
    public partial class Hub
    {
        public class Instruction
        {
            public class AddResourceToHub : SimulationMessage
            {
                public static AddResourceToHub Create(FAgentInformation message, IActorRef target, bool logThis = false)
                {
                    return new AddResourceToHub(message: message, target: target, logThis: logThis);
                }
                private AddResourceToHub(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
                {

                }
                public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
            }

            public class ProductionStarted : SimulationMessage
            {
                public static ProductionStarted Create(Guid message, IActorRef target)
                {
                    return new ProductionStarted(message: message, target: target);
                }
                private ProductionStarted(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public Guid GetObjectfromMessage { get => (Guid)Message; }
            }

            public class EnqueueJob : SimulationMessage
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

            public class ProposalFromResource : SimulationMessage
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

            public class SetWorkItemStatus : SimulationMessage
            {
                public static SetWorkItemStatus Create(Guid message, IActorRef target)
                {
                    return new SetWorkItemStatus(message: message, target: target);
                }
                private SetWorkItemStatus(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }

            public class FinishJob : SimulationMessage
            {
                public static FinishJob Create(IJobResult message, IActorRef target)
                {
                    return new FinishJob(message: message, target: target);
                }
                private FinishJob(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public IJobResult GetObjectFromMessage { get => Message as IJobResult; }
            }
        }
    }
}