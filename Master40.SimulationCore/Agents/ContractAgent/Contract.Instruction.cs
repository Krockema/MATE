using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using Master40.DB.DataModel;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.ContractAgent
{
    public partial class Contract
    {
        public class Instruction { 
            public class StartOrder : SimulationMessage
            {
                private StartOrder(object message, IActorRef target, bool logThis, Priority priority = Priority.Medium) : base(message, target, logThis, priority)
                {
                }

                public static ISimulationMessage Create(OrderPart message, IActorRef target, bool logThis = false)
                {
                    return new StartOrder(message, target, logThis);
                }
                public OrderPart GetObjectFromMessage { get => Message as OrderPart; }
            }

            public class Finish : SimulationMessage
            {
                private Finish(object message, IActorRef target, bool logThis, Priority priority = Priority.Medium) : base(message, target, logThis, priority)
                {
                }

                public static ISimulationMessage Create(FRequestItem requestItem, IActorRef target, bool logThis)
                {
                    return new Finish(requestItem, target, logThis);
                }
                public FRequestItem GetObjectFromMessage { get => Message as FRequestItem; }

            }
        }
    }
}
