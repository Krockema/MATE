using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.DataModel;
using static FArticles;
using static FStockReservations;

namespace Master40.SimulationCore.Agents.DispoAgent
{
    public partial class Dispo
    {
        public class Instruction
        {
            public class RequestArticle : SimulationMessage
            {
                public static RequestArticle Create(FArticle message, IActorRef target)
                {
                    return new RequestArticle(message: message, target: target);
                }
                private RequestArticle(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FArticle GetObjectFromMessage { get => Message as FArticle; }
            }

            public class ResponseFromStock : SimulationMessage
            {
                public static ResponseFromStock Create(FStockReservation message, IActorRef target)
                {
                    return new ResponseFromStock(message: message, target: target);
                }
                private ResponseFromStock(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public FStockReservation GetObjectFromMessage { get => this.Message as FStockReservation; }
            }

            public class ResponseFromSystemForBom : SimulationMessage
            {
                public static ResponseFromSystemForBom Create(M_Article message, IActorRef target)
                {
                    return new ResponseFromSystemForBom(message: message, target: target);
                }
                private ResponseFromSystemForBom(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public M_Article GetObjectFromMessage { get => Message as M_Article; }
            }
            public class WithdrawArticleFromStock : SimulationMessage
            {
                public static WithdrawArticleFromStock Create(object message, IActorRef target)
                {
                    return new WithdrawArticleFromStock(message: message, target: target);
                }
                private WithdrawArticleFromStock(object message, IActorRef target) : base(message: message, target: target)
                {

                }
            }
        }
    }
}
