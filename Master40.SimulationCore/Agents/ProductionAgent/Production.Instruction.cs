using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.ProductionAgent
{
    public partial class Production
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
                public static ProductionStarted Create(FWorkItem message, IActorRef target)
                {
                    return new ProductionStarted(message, target);
                }
                private ProductionStarted(object message, IActorRef target) : base(message, target)
                {

                }
                public FWorkItem GetObjectFromMessage { get => Message as FWorkItem; }
            }

            public class StartProduction : SimulationMessage
            {
                public static StartProduction Create(FRequestItem message, IActorRef target)
                {
                    return new StartProduction(message, target);
                }
                private StartProduction(object message, IActorRef target) : base(message, target)
                {

                }
                public FRequestItem GetObjectFromMessage { get => Message as FRequestItem; }
            }

            public class Finished : SimulationMessage
            {
                public static Finished Create(FItemStatus message, IActorRef target)
                {
                    return new Finished(message, target);
                }
                private Finished(object message, IActorRef target) : base(message, target)
                {

                }
                public FItemStatus GetObjectFromMessage { get => Message as FItemStatus; }
            }
            public class SetHubAgent : SimulationMessage
            {
                public static SetHubAgent Create(FHubInformation message, IActorRef target)
                {
                    return new SetHubAgent(message, target);
                }
                private SetHubAgent(object message, IActorRef target) : base(message, target)
                {

                }
                public FHubInformation GetObjectFromMessage { get => Message as FHubInformation; }
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
            public class ProvideRequest : SimulationMessage
            {
                public static ProvideRequest Create(FItemStatus message, IActorRef target)
                {
                    return new ProvideRequest(message, target);
                }
                private ProvideRequest(object message, IActorRef target) : base(message, target)
                {

                }
                public FItemStatus GetObjectFromMessage { get => Message as FItemStatus; }
            }
        }
    }
}