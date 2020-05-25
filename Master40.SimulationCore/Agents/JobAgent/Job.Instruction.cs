using Akka.Actor;
using AkkaSim.Definitions;
using static FJobConfirmations;
using static FJobResourceConfirmations;

namespace Master40.SimulationCore.Agents.JobAgent
{
    public partial class Job
    {
        public class Instruction
        {
            public class AcknowledgeRevoke : SimulationMessage
            {
                public static AcknowledgeRevoke Create(IActorRef message, IActorRef target)
                {
                    return new AcknowledgeRevoke(message: message, target: target);
                }
                private AcknowledgeRevoke(IActorRef message, IActorRef target) : base(message: message, target: target)
                { }
                public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
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
            
            public class FinishProcessing : SimulationMessage
            {
                public static FinishProcessing Create(IActorRef message, IActorRef target)
                {
                    return new FinishProcessing(message: message, target: target);
                }
                private FinishProcessing(object message, IActorRef target) : base(message: message, target: target)
                { }
                public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
            }

            public class RequestProcessingStart : SimulationMessage
            {
                public static RequestProcessingStart Create(IActorRef message, IActorRef target)
                {
                    return new RequestProcessingStart(message: message, target: target);
                }
                private RequestProcessingStart(object message, IActorRef target) : base(message: message, target: target)
                { }

                public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
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
                public static RequestSetupStart Create(IActorRef message, IActorRef target)
                {
                    return new RequestSetupStart(message: message, target: target);
                }
                private RequestSetupStart(object message, IActorRef target) : base(message: message, target: target)
                { }

                public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
            }

            public class TerminateJob : SimulationMessage
            {
                public static TerminateJob Create(IActorRef target)
                {
                    return new TerminateJob(target: target);
                }
                private TerminateJob(IActorRef target) : base(message: null, target: target)
                { }
            }

            public class DelayedStartNotification : SimulationMessage
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
