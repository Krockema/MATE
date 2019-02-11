using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public partial class Hub
    {
        public class Instruction
        {
            public class AddMachineToHub: SimulationMessage
            {
                public static AddMachineToHub Create(FHubInformation message, IActorRef target)
                {
                    return new AddMachineToHub(message, target);
                }
                private AddMachineToHub(object message, IActorRef target) : base(message, target)
                {

                }
                public FHubInformation GetObjectFromMessage { get => Message as FHubInformation; }
            }

            public class ProductionStarted : SimulationMessage
            {
                public static ProductionStarted Create(FWorkItem message, IActorRef target)
                {
                    return new ProductionStarted(message, target);
                }
                private ProductionStarted(object message, IActorRef target) : base(message, target)
                {

                }
                public FWorkItem GetObjectfromMessage { get => Message as FWorkItem; }
            }

            public class EnqueueWorkItem : SimulationMessage
            {
                public static EnqueueWorkItem Create(FWorkItem message, IActorRef target)
                {
                    return new EnqueueWorkItem(message, target);
                }
                private EnqueueWorkItem(object message, IActorRef target) : base(message, target)
                {

                }
                public FWorkItem GetObjectFromMessage { get => Message as FWorkItem; }
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
                public static SetWorkItemStatus Create(FItemStatus message, IActorRef target)
                {
                    return new SetWorkItemStatus(message, target);
                }
                private SetWorkItemStatus(object message, IActorRef target) : base(message, target)
                {

                }
                public FItemStatus GetObjectFromMessage { get => Message as FItemStatus; }
            }

            public class FinishWorkItem : SimulationMessage
            {
                public static FinishWorkItem Create(FWorkItem message, IActorRef target)
                {
                    return new FinishWorkItem(message, target);
                }
                private FinishWorkItem(object message, IActorRef target) : base(message, target)
                {

                }
                public FWorkItem GetObjectFromMessage { get => Message as FWorkItem; }
            }

        }
    }
}
