using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using static FArticles;
using static FStockReservations;

namespace Mate.Production.Core.Agents.DispoAgent
{
    public partial class Dispo
    {
        public record Instruction
        {
            public record RequestArticle : HiveMessage
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

            public record ResponseFromStock : HiveMessage
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

            public record ResponseFromSystemForBom : HiveMessage
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
            public record WithdrawArticleFromStock : HiveMessage
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
