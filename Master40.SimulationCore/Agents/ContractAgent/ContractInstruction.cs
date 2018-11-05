using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using Master40.DB.Models;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents
{
    public partial class ContractAgent
    {
        //Posible Properties
        public const string RequestItem = "RequestItem";


        public class Instruction { 
            public class StartOrder : SimulationMessage
            {
                private StartOrder(object message, IActorRef target, Priority priority = Priority.Medium) : base(message, target, priority)
                {
                }

                public static ISimulationMessage Create(OrderPart message, IActorRef target)
                {
                    return new StartOrder(message, target);
                }
                public OrderPart GetObjectFromMessage { get => Message as OrderPart; }
            }

            public class Finish : SimulationMessage
            {
                private Finish(object message, IActorRef target, Priority priority = Priority.Medium) : base(message, target, priority)
                {
                }

                public static ISimulationMessage Create(RequestItem requestItem, IActorRef target)
                {
                    return new Finish(requestItem, target);
                }
                public RequestItem GetObjectFromMessage { get => Message as RequestItem; }

            }
        }
    }
}
