using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public partial class StorageAgent
    {
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
            public class ProvideArticleAtDue : SimulationMessage
            {
                public static ProvideArticleAtDue Create(RequestItem message, IActorRef target)
                {
                    return new ProvideArticleAtDue(message, target);
                }
                private ProvideArticleAtDue(object message, IActorRef target) : base(message, target)
                {
                }
                public RequestItem GetObjectFromMessage { get => Message as RequestItem; }
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
                public static ResponseFromProduction Create(RequestItem message, IActorRef target)
                {
                    return new ResponseFromProduction(message, target);
                }
                private ResponseFromProduction(object message, IActorRef target) : base(message, target)
                {
                }
                public RequestItem GetObjectFromMessage { get => Message as RequestItem; }
            }
        }
    }
}
