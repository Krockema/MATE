using System;
using Akka.Actor;
using AkkaSim.Definitions;
using static FArticles;
using static FProductionResults;

namespace Master40.SimulationCore.Agents.StorageAgent
{
    public partial class Storage
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
            public class ProvideArticleAtDue : SimulationMessage
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
            public class StockRefill : SimulationMessage
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
            public class WithdrawArticle : SimulationMessage
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
            
            public class ResponseFromProduction : SimulationMessage
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
