using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent.Types;
using static FArticles;
using static FJobConfirmations;
using static FJobResourceConfirmations;
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

            public class AcknowledgeJob : SimulationMessage
            {
                public static AcknowledgeJob Create(FJobResourceConfirmation message,IActorRef target)
                {
                    return new AcknowledgeJob(message: message, target: target);
                }
                private AcknowledgeJob(object message, IActorRef target) : base(message: message, target: target)
                { }
                public FJobResourceConfirmation GetObjectFromMessage { get => Message as FJobResourceConfirmation; }
            }

            public class LockJob : SimulationMessage
            {
                public static LockJob Create(FJobConfirmation message, IActorRef target)
                {
                    return new LockJob(message: message, target: target);
                }
                private LockJob(object message, IActorRef target) : base(message: message, target: target)
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

            public class StartProcessing : SimulationMessage
            {
                public static StartProcessing Create(IActorRef target)
                {
                    return new StartProcessing(target: target);
                }
                private StartProcessing(IActorRef target) : base(message: null, target: target)
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

            public class AcceptAcknowledgeResponseFromResource : SimulationMessage
            {
                public static AcceptAcknowledgeResponseFromResource Create(IActorRef target)
                {
                    return new AcceptAcknowledgeResponseFromResource(target: target);
                }
                private AcceptAcknowledgeResponseFromResource(IActorRef target) : base(message: null, target: target)
                {

                }

            }

            public class RejectAcknowledgeResponseFromResource : SimulationMessage
            {
                public static RejectAcknowledgeResponseFromResource Create(IActorRef target)
                {
                    return new RejectAcknowledgeResponseFromResource(target: target);
                }
                private RejectAcknowledgeResponseFromResource(IActorRef target) : base(message: null, target: target)
                {
                }
            }

            public class StartRequeue : SimulationMessage
            {
                public static StartRequeue Create(IActorRef target)
                {
                    return new StartRequeue(target: target);
                }
                private StartRequeue(IActorRef target) : base(message: null, target: target)
                {
                }
            }
            

        }
    }
}
