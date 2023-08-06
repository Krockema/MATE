using Akka.Actor;
using Akka.Hive.Definitions;
using static FJobResourceConfirmations;

namespace Mate.Production.Core.Agents.JobAgent
{
    public partial class Job
    {
        public record Instruction
        {
            public record AcknowledgeRevoke : HiveMessage
            {
                public static AcknowledgeRevoke Create(IActorRef message, IActorRef target)
                {
                    return new AcknowledgeRevoke(message: message, target: target);
                }
                private AcknowledgeRevoke(IActorRef message, IActorRef target) : base(message: message, target: target)
                { }
                public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
            }

            public record RequestDissolve : HiveMessage
            {
                public static RequestDissolve Create(IActorRef target)
                {
                    return new RequestDissolve(target: target);
                }
                private RequestDissolve(IActorRef target) : base(message: null, target: target)
                { }
            }

            public record StartRequeue : HiveMessage
            {
                public static StartRequeue Create(IActorRef target)
                {
                    return new StartRequeue(target: target);
                }
                private StartRequeue(IActorRef target) : base(message: null, target: target)
                {
                }
            }

            public record AcknowledgeJob : HiveMessage
            {
                public static AcknowledgeJob Create(FJobResourceConfirmation message,IActorRef target)
                {
                    return new AcknowledgeJob(message: message, target: target);
                }
                private AcknowledgeJob(object message, IActorRef target) : base(message: message, target: target)
                { }
                public FJobResourceConfirmation GetObjectFromMessage { get => Message as FJobResourceConfirmation; }
            }

            public record ResourceWillBeReady : HiveMessage
            {
                public static ResourceWillBeReady Create(IActorRef target)
                {
                    return new ResourceWillBeReady(message: null, target: target);
                }
                private ResourceWillBeReady(object message, IActorRef target) : base(message: message, target: target)
                { }
            }


            public record FinishProcessing : HiveMessage
            {
                public static FinishProcessing Create(IActorRef message, IActorRef target)
                {
                    return new FinishProcessing(message: message, target: target);
                }
                private FinishProcessing(object message, IActorRef target) : base(message: message, target: target)
                { }
                public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
            }

            public record RequestProcessingStart : HiveMessage
            {
                public static RequestProcessingStart Create(IActorRef message, IActorRef target)
                {
                    return new RequestProcessingStart(message: message, target: target);
                }
                private RequestProcessingStart(object message, IActorRef target) : base(message: message, target: target)
                { }

                public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
            }

            public record BucketIsFixed : HiveMessage
            {
                public static BucketIsFixed Create(IActorRef target)
                {
                    return new BucketIsFixed(target: target);
                }
                private BucketIsFixed(IActorRef target) : base(message: null, target: target)
                { }
            }

            public record RequestSetupStart : HiveMessage
            {
                public static RequestSetupStart Create(IActorRef message, IActorRef target)
                {
                    return new RequestSetupStart(message: message, target: target);
                }
                private RequestSetupStart(object message, IActorRef target) : base(message: message, target: target)
                { }

                public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
            }

            public record TerminateJob : HiveMessage
            {
                public static TerminateJob Create(IActorRef target)
                {
                    return new TerminateJob(target: target);
                }
                private TerminateJob(IActorRef target) : base(message: null, target: target)
                { }
            }

            public record DelayedStartNotification : HiveMessage
            {
                public static DelayedStartNotification Create(IActorRef target)
                {
                    return new DelayedStartNotification(message: null, target: target);
                }
                private DelayedStartNotification(object message, IActorRef target) : base(message: message, target: target)
                {

                }
            }
        }
    }
}
