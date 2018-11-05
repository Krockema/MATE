using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public partial class ProductionAgent
    {
        public class Instruction
        {
            /// <summary>
            ///      Finished,
            ///      ProductionStarted
            ///      SetComunicationAgent
            /// </summary>
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
            
            public class Finished : SimulationMessage
            {
                public static Finished Create(ItemStatus message, IActorRef target)
                {
                    return new Finished(message, target);
                }
                private Finished(object message, IActorRef target) : base(message, target)
                {

                }
                public ItemStatus GetObjectFromMessage { get => Message as ItemStatus; }
            }
            public class SetHubAgent : SimulationMessage
            {
                public static SetHubAgent Create(HubInformation message, IActorRef target)
                {
                    return new SetHubAgent(message, target);
                }
                private SetHubAgent(object message, IActorRef target) : base(message, target)
                {

                }
                public HubInformation GetObjectFromMessage { get => Message as HubInformation; }
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
            public class ProvideRequest : SimulationMessage
            {
                public static ProvideRequest Create(ItemStatus message, IActorRef target)
                {
                    return new ProvideRequest(message, target);
                }
                private ProvideRequest(object message, IActorRef target) : base(message, target)
                {

                }
                public ItemStatus GetObjectFromMessage { get => Message as ItemStatus; }
            }
        }
    }
}
