using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records;

namespace Mate.Production.Core.Agents.DispoAgent
{
    public partial class Dispo
    {
        public record Instruction
        {
            public record RequestArticle : HiveMessage
            {
                public static RequestArticle Create(ArticleRecord message, IActorRef target)
                {
                    return new RequestArticle(message: message, target: target);
                }
                private RequestArticle(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public ArticleRecord GetObjectFromMessage { get => Message as ArticleRecord; }
            }

            public record ResponseFromStock : HiveMessage
            {
                public static ResponseFromStock Create(StockReservationRecord message, IActorRef target)
                {
                    return new ResponseFromStock(message: message, target: target);
                }
                private ResponseFromStock(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public StockReservationRecord GetObjectFromMessage { get => this.Message as StockReservationRecord; }
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
