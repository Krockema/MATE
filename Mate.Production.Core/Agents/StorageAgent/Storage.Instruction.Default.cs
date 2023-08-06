using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using static FArticles;
using static FProductionResults;

namespace Mate.Production.Core.Agents.StorageAgent
{
    public partial class Storage
    {
        public partial class Instruction
        {

            public record Default
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
            public record ProvideArticleAtDue : HiveMessage
            {
                public static ProvideArticleAtDue Create(Guid message, IActorRef target)
                {
                    return new ProvideArticleAtDue(message: message, target: target);
                }
                private ProvideArticleAtDue(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }
            public record StockRefill : HiveMessage
            {
                public static StockRefill Create(Guid message, IActorRef target)
                {
                    return new StockRefill(message: message, target: target);
                }
                private StockRefill(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }

            }
            public record WithdrawArticle : HiveMessage
            {
                public static WithdrawArticle Create(Guid message, IActorRef target)
                {
                    return new WithdrawArticle(message: message, target: target);
                }
                private WithdrawArticle(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }
            
            public record ResponseFromProduction : HiveMessage
            {
                public static ResponseFromProduction Create(FProductionResult message, IActorRef target)
                {
                    return new ResponseFromProduction(message: message, target: target);
                }
                private ResponseFromProduction(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FProductionResult GetObjectFromMessage { get => Message as FProductionResult; }
            }

            }
        }
    }
}
