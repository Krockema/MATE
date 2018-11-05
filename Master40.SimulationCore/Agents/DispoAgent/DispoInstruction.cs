using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.Models;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents
{
    public partial class DispoAgent
    {
        public const string RequestItem = "RequestItem";
        public const string QuantityToProduce = "QuantityToProduce";
        public const string StockAgentRef = "StockAgentRef";

        public class Instruction
        {
            public class RequestArticle : SimulationMessage
            {
                public static RequestArticle Create(RequestItem message, IActorRef target)
                {
                    return new RequestArticle(message, target);
                }
                private RequestArticle(object message, IActorRef target) : base(message, target)
                {
                }
                public RequestItem GetObjectFromMessage { get => Message as RequestItem; }
            }

            public class ResponseFromStock : SimulationMessage
            {
                public static ResponseFromStock Create(StockReservation message, IActorRef target)
                {
                    return new ResponseFromStock(message, target);
                }
                private ResponseFromStock(object message, IActorRef target) : base(message, target)
                {

                }
                public StockReservation GetObjectFromMessage { get => this.Message as StockReservation; }
            }
            public class RequestProvided : SimulationMessage
            {
                public static RequestProvided Create(RequestItem message, IActorRef target)
                {
                    return new RequestProvided(message, target);
                }
                private RequestProvided(object message, IActorRef target) : base(message, target)
                {

                }
                public RequestItem GetObjectFromMessage { get => Message as RequestItem; }
            }
            public class ResponseFromSystemForBom : SimulationMessage
            {
                public static ResponseFromSystemForBom Create(Article message, IActorRef target)
                {
                    return new ResponseFromSystemForBom(message, target);
                }
                private ResponseFromSystemForBom(object message, IActorRef target) : base(message, target)
                {
                }
                public Article GetObjectFromMessage { get => Message as Article; }
            }
            public class WithdrawMaterialsFromStock : SimulationMessage
            {
                public static WithdrawMaterialsFromStock Create(object message, IActorRef target)
                {
                    return new WithdrawMaterialsFromStock(message, target);
                }
                private WithdrawMaterialsFromStock(object message, IActorRef target) : base(message, target)
                {

                }
            }
        }
    }
}
