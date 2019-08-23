using Akka.Actor;
using AkkaSim.Definitions;
using System;
using static FArticles;

namespace Master40.SimulationCore.Agents.ProductionAgent
{
    public partial class Production
    {
        public class Instruction
        {
            /// <summary>
            ///      Finished,
            ///      ProductionStarted
            ///      SetComunicationAgent
            /// </summary>

            public class StartProduction : SimulationMessage
            {
                public static StartProduction Create(FArticle message, IActorRef target)
                {
                    return new StartProduction(message: message, target: target);
                }
                private StartProduction(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public FArticle GetObjectFromMessage { get => Message as FArticle; }
            }

            public class Finished : SimulationMessage
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