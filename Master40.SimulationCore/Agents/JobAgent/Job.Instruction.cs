using Akka.Actor;
using AkkaSim.Definitions;
using static FArticles;
using static FJobConfirmations;
using static FJobResourceConfirmations;
using static IJobs;

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

            public class RequestDissolve : SimulationMessage
            {
                public static RequestDissolve Create(IActorRef target)
                {
                    return new RequestDissolve(target: target);
                }
                private RequestDissolve(IActorRef target) : base(message: null, target: target)
                { }
            }

            public class AcknowledgeDissolve : SimulationMessage
            {
                public static AcknowledgeDissolve Create(IActorRef target)
                {
                    return new AcknowledgeDissolve(target: target);
                }
                private AcknowledgeDissolve(IActorRef target) : base(message: null, target: target)
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

            public class FinalBucket : SimulationMessage
            {
                public static FinalBucket Create(FJobConfirmation job, IActorRef target)
                {
                    return new FinalBucket(job, target: target);
                }
                private FinalBucket(FJobConfirmation job, IActorRef target) : base(message: job, target: target)
                {
                }
                public FJobConfirmation GetObjectFromMessage { get => Message as FJobConfirmation; }
            }

            public class FinishSetup : SimulationMessage
            {
                public static FinishSetup Create(IActorRef target)
                {
                    return new FinishSetup(message: null, target: target);
                }
                private FinishSetup(object message, IActorRef target) : base(message: message, target: target)
                { }
            }

            public class FinishProcessing : SimulationMessage
            {
                public static FinishProcessing Create(IActorRef target)
                {
                    return new FinishProcessing(message: null, target: target);
                }
                private FinishProcessing(object message, IActorRef target) : base(message: message, target: target)
                { }
            }

            public class RequestProcessingStart : SimulationMessage
            {
                public static RequestProcessingStart Create(IActorRef target)
                {
                    return new RequestProcessingStart(target: target);
                }
                private RequestProcessingStart(IActorRef target) : base(message: null, target: target)
                { }
            }

            public class BucketIsFixed : SimulationMessage
            {
                public static BucketIsFixed Create(IActorRef target)
                {
                    return new BucketIsFixed(target: target);
                }
                private BucketIsFixed(IActorRef target) : base(message: null, target: target)
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
