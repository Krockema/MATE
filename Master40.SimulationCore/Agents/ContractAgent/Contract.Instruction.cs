using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using Master40.DB.DataModel;
using static FArticles;

namespace Master40.SimulationCore.Agents.ContractAgent
{
    public partial class Contract
    {
        public class Instruction { 
            public class StartOrder : SimulationMessage
            {
                private StartOrder(object message, IActorRef target, bool logThis, Priority priority = Priority.Medium) : base(message: message, target: target, logThis: logThis, priority: priority)
                {
                }

                public static ISimulationMessage Create(T_CustomerOrderPart message, IActorRef target, bool logThis = false)
                {
                    return new StartOrder(message: message, target: target, logThis: logThis);
                }
                public T_CustomerOrderPart GetObjectFromMessage { get => Message as T_CustomerOrderPart; }
            }

            public class Finish : SimulationMessage
            {
                private Finish(object message, IActorRef target, bool logThis, Priority priority = Priority.Medium) : base(message: message, target: target, logThis: logThis, priority: priority)
                {
                }

                public static ISimulationMessage Create(FArticle requestItem, IActorRef target, bool logThis)
                {
                    return new Finish(message: requestItem, target: target, logThis: logThis);
                }
                public FArticle GetObjectFromMessage { get => Message as FArticle; }

            }
        }
    }
}
