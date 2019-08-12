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
                    return new RequestArticle(message, target);
                }
                private RequestArticle(object message, IActorRef target) : base(message, target)
                {
                }
                public FArticle GetObjectFromMessage { get => Message as FArticle; }
            }
            public class ProvideArticleAtDue : SimulationMessage
            {
                public static ProvideArticleAtDue Create(Guid message, IActorRef target)
                {
                    return new ProvideArticleAtDue(message, target);
                }
                private ProvideArticleAtDue(object message, IActorRef target) : base(message, target)
                {
                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }
            public class StockRefill : SimulationMessage
            {
                public static StockRefill Create(Guid message, IActorRef target)
                {
                    return new StockRefill(message, target);
                }
                private StockRefill(object message, IActorRef target) : base(message, target)
                {
                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }

            }
            public class WithdrawlMaterial : SimulationMessage
            {
                public static WithdrawlMaterial Create(Guid message, IActorRef target)
                {
                    return new WithdrawlMaterial(message, target);
                }
                private WithdrawlMaterial(object message, IActorRef target) : base(message, target)
                {
                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }
            
            public class ResponseFromProduction : SimulationMessage
            {
                public static ResponseFromProduction Create(FProductionResult message, IActorRef target)
                {
                    return new ResponseFromProduction(message, target);
                }
                private ResponseFromProduction(object message, IActorRef target) : base(message, target)
                {
                }
                public FProductionResult GetObjectFromMessage { get => Message as FProductionResult; }
            }
        }
    }
}
