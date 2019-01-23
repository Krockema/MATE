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
                public static AddMachineToHub Create(HubInformation message, IActorRef target)
                {
                    return new AddMachineToHub(message, target);
                }
                private AddMachineToHub(object message, IActorRef target) : base(message, target)
                {

                }
                public HubInformation GetObjectFromMessage { get => Message as HubInformation; }
            }

            public class ProductionStarted : SimulationMessage
            {
                public static ProductionStarted Create(WorkItem message, IActorRef target)
                {
                    return new ProductionStarted(message, target);
                }
                private ProductionStarted(object message, IActorRef target) : base(message, target)
                {

                }
                public WorkItem GetObjectfromMessage { get => Message as WorkItem; }
            }

            public class EnqueueWorkItem : SimulationMessage
            {
                public static EnqueueWorkItem Create(WorkItem message, IActorRef target)
                {
                    return new EnqueueWorkItem(message, target);
                }
                private EnqueueWorkItem(object message, IActorRef target) : base(message, target)
                {

                }
                public WorkItem GetObjectFromMessage { get => Message as WorkItem; }
            }

            public class ProposalFromMachine : SimulationMessage
            {
                public static ProposalFromMachine Create(Proposal message, IActorRef target)
                {
                    return new ProposalFromMachine(message, target);
                }
                private ProposalFromMachine(object message, IActorRef target) : base(message, target)
                {

                }
                public Proposal GetObjectFromMessage { get => Message as Proposal; }
            }

            public class SetWorkItemStatus : SimulationMessage
            {
                public static SetWorkItemStatus Create(ItemStatus message, IActorRef target)
                {
                    return new SetWorkItemStatus(message, target);
                }
                private SetWorkItemStatus(object message, IActorRef target) : base(message, target)
                {

                }
                public ItemStatus GetObjectFromMessage { get => Message as ItemStatus; }
            }

            public class FinishWorkItem : SimulationMessage
            {
                public static FinishWorkItem Create(WorkItem message, IActorRef target)
                {
                    return new FinishWorkItem(message, target);
                }
                private FinishWorkItem(object message, IActorRef target) : base(message, target)
                {

                }
                public WorkItem GetObjectFromMessage { get => Message as WorkItem; }
            }

        }
    }
}
