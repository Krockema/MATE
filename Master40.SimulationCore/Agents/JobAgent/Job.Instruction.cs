using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.DataModel;
using static FArticles;
using static FStockReservations;

namespace Master40.SimulationCore.Agents.JobAgent
{
    public partial class Job
    {
        public class Instruction
        {
            /// <summary>
            /// Terminates the Job, send from Hub
            /// </summary>
            public class TerminateJob : SimulationMessage
            {
                public static TerminateJob Create(IActorRef target)
                {
                    return new TerminateJob(target: target);
                }
                private TerminateJob(IActorRef target) : base(message: null, target: target)
                { }
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
                public FArticle GetObjectFromMessage { get => Message as FArticle; }

            }
        }
    }
}
