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
            public class AddMachineToHub : SimulationMessage
            {
                public static AddMachineToHub Create(FAgentInformation message, IActorRef target, bool logThis = false)
                {
                    return new AddMachineToHub(message, target, logThis);
                }
                private AddMachineToHub(object message, IActorRef target, bool logThis) : base(message, target, logThis)
                {

                }
                public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
            }

            public class ProductionStarted : SimulationMessage
            {
                public static ProductionStarted Create(Guid message, IActorRef target)
                {
                    return new ProductionStarted(message, target);
                }
                private ProductionStarted(object message, IActorRef target) : base(message, target)
                {

                }
                public Guid GetObjectfromMessage { get => (Guid)Message; }
            }

            public class EnqueueJob : SimulationMessage
            {
                public static EnqueueJob Create(IJob message, IActorRef target)
                {
                    return new EnqueueJob(message, target);
                }
                private EnqueueJob(object message, IActorRef target) : base(message, target)
                {

                }
                public IJob GetObjectFromMessage { get => Message as IJob; }
            }

            public class ProposalFromMachine : SimulationMessage
            {
                public static ProposalFromMachine Create(FProposal message, IActorRef target)
                {
                    return new ProposalFromMachine(message, target);
                }
                private ProposalFromMachine(object message, IActorRef target) : base(message, target)
                {

                }
                public FProposal GetObjectFromMessage { get => Message as FProposal; }
            }

            public class SetWorkItemStatus : SimulationMessage
            {
                public static SetWorkItemStatus Create(Guid message, IActorRef target)
                {
                    return new SetWorkItemStatus(message, target);
                }
                private SetWorkItemStatus(object message, IActorRef target) : base(message, target)
                {

                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }

            public class FinishJob : SimulationMessage
            {
                public static FinishJob Create(IJobResult message, IActorRef target)
                {
                    return new FinishJob(message, target);
                }
                private FinishJob(object message, IActorRef target) : base(message, target)
                {

                }
                public IJobResult GetObjectFromMessage { get => Message as IJobResult; }
            }
        }
    }
}