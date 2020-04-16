using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent.Types;
using static FArticles;
using static FJobConfirmations;
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

            public class UpdateJob : SimulationMessage
            {
                public static UpdateJob Create(M_ResourceCapabilityProvider message,IActorRef target)
                {
                    return new UpdateJob(message: message, target: target);
                }
                private UpdateJob(object message, IActorRef target) : base(message: null, target: target)
                { }
                public M_ResourceCapabilityProvider GetObjectFromMessage { get => Message as M_ResourceCapabilityProvider; }
            }

            public class LockJob : SimulationMessage
            {
                public static LockJob Create(FJobConfirmation message, IActorRef target)
                {
                    return new LockJob(message: message, target: target);
                }
                private LockJob(object message, IActorRef target) : base(message: null, target: target)
                { }
                public FJobConfirmation GetObjectFromMessage { get => Message as FJobConfirmation; }
            }

            public class RequestJobStart : SimulationMessage
            {
                public static RequestJobStart Create(IActorRef target)
                {
                    return new RequestJobStart(target: target);
                }
                private RequestJobStart(IActorRef target) : base(message: null, target: target)
                { }
            }
            public class RequestSetupStart : SimulationMessage
            {
                public static RequestSetupStart Create(IActorRef target)
                {
                    return new RequestSetupStart(target: target);
                }
                private RequestSetupStart(IActorRef target) : base(message: null, target: target)
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
