using System;
using Akka.Actor;
using Akka.Hive.Definitions;
namespace Mate.Production.Core.Agents.ProductionAgent
{
    public partial class Production
    {
        public record Instruction
        {
            /// <summary>
            ///      Finished,
            ///      ProductionStarted
            ///      SetComunicationAgent
            /// </summary>

            public record StartProduction : HiveMessage
            {
                public static StartProduction Create(ArticleRecord message, IActorRef target)
                {
                    return new StartProduction(message: message, target: target);
                }
                private StartProduction(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public ArticleRecord GetObjectFromMessage { get => Message as ArticleRecord; }
            }

            public record Finished : HiveMessage
            {
                public static Finished Create(Guid message, IActorRef target)
                {
                    return new Finished(message: message, target: target);
                }
                private Finished(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
            }
        }
    }
}