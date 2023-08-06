using System;
using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Types;
using static FAgentInformations;
using static FArticleProviders;
using static FBreakDowns;
using static FUpdateStartConditions;
using static IConfirmations;
using static IJobResults;
using static IJobs;

namespace Mate.Production.Core.Agents
{
    public record BasicInstruction
    {
        public record Initialize : HiveMessage
        {
            public static Initialize Create(IActorRef target, IBehaviour message = null)
            {
                return new Initialize(message: message, target: target);
            }
            private Initialize(object message, IActorRef target) : base(message: message, target: target)
            {
            }
            public IBehaviour GetObjectFromMessage { get => Message as IBehaviour; }
        }

        public record ChildRef : HiveMessage
        {
            public static ChildRef Create(IActorRef message, IActorRef target)
            {
                return new ChildRef(message: message, target: target);
            }
            private ChildRef(object message, IActorRef target) : base(message: message, target: target)
            {
            }
            public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
        }
        public record JobForwardEnd : HiveMessage
        {
            public static JobForwardEnd Create(long message, IActorRef target)
            {
                return new JobForwardEnd(message: message, target: target);
            }
            private JobForwardEnd(object message, IActorRef target) : base(message: message, target: target)
            {
            }
            public long GetObjectFromMessage { get => (long)Message; }
        }

        public record ResponseFromDirectory : HiveMessage
        {
            public static ResponseFromDirectory Create(FAgentInformation message, IActorRef target)
            {
                return new ResponseFromDirectory(message: message, target: target);
            }
            private ResponseFromDirectory(object message, IActorRef target) : base(message: message, target: target)
            {
            }
            public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
        }

        public record ProvideArticle : HiveMessage
        {
            public static ProvideArticle Create(FArticleProvider message, IActorRef target, bool logThis)
            {
                return new ProvideArticle(message: message, target: target, logThis: logThis);
            }
            private ProvideArticle(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
            {

            }
            public FArticleProvider GetObjectFromMessage { get => Message as FArticleProvider; }
        }

        public record ResourceBrakeDown : HiveMessage
        {
            public static ResourceBrakeDown Create(FBreakDown message, IActorRef target, bool logThis = false)
            {
                return new ResourceBrakeDown(message: message, target: target, logThis: logThis);
            }
            private ResourceBrakeDown(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
            {
            }
            public FBreakDown GetObjectFromMessage { get => Message as FBreakDown; }
        }

        public record RemoveVirtualChild : HiveMessage
        {
            public static RemoveVirtualChild Create(IActorRef target, bool logThis = false)
            {
                return new RemoveVirtualChild(message: null, target: target, logThis: logThis);
            }
            private RemoveVirtualChild(object message, IActorRef target, bool logThis) 
                : base(message: message, target: target, logThis: logThis)
            {
            }
        }

        public record RemovedChildRef : HiveMessage
        {
            public static RemovedChildRef Create(IActorRef target, bool logThis = false)
            {
                return new RemovedChildRef(message: null, target: target, logThis: logThis);
            }
            private RemovedChildRef(object message, IActorRef target, bool logThis) 
                : base(message: message, target: target, logThis: logThis)
            {
            }
        }
        

        public record WithdrawRequiredArticles : HiveMessage
        {
            public static WithdrawRequiredArticles Create(Guid message, IActorRef target)
            {
                return new WithdrawRequiredArticles(message: message, target: target);
            }
            private WithdrawRequiredArticles(object message, IActorRef target) : base(message: message, target: target)
            {

            }
            public Guid GetObjectFromMessage { get => (Guid)Message; }
        }

        public record FinishJob : HiveMessage
        {
            public static FinishJob Create(IJobResult message, IActorRef target)
            {
                return new FinishJob(message: message, target: target);
            }
            private FinishJob(object message, IActorRef target) : base(message: message, target: target)
            {
            }
            public IJobResult GetObjectFromMessage { get => Message as IJobResult; }
        }
        public record FinishOperation : HiveMessage
        {
            public static FinishOperation Create(IJobResult message, IActorRef target)
            {
                return new FinishOperation(message: message, target: target);
            }
            private FinishOperation(object message, IActorRef target) : base(message: message, target: target)
            {
            }
            public IJobResult GetObjectFromMessage { get => Message as IJobResult; }
        }
        public record UpdateStartConditions : HiveMessage
        {
            public static UpdateStartConditions Create(FUpdateStartCondition message, IActorRef target)
            {
                return new UpdateStartConditions(message: message, target: target);
            }
            private UpdateStartConditions(object message, IActorRef target) : base(message: message, target: target)
            {
            }
            public FUpdateStartCondition GetObjectFromMessage { get => Message as FUpdateStartCondition; }
        }

        public record UpdateJob : HiveMessage
        {
            public static UpdateJob Create(IJob message, IActorRef target)
            {
                return new UpdateJob(message: message, target: target);
            }
            private UpdateJob(IJob message, IActorRef target) : base(message: message, target: target)
            {
            }
            public IJob GetObjectFromMessage => Message as IJob; 
        }

        public record FinalBucket : HiveMessage
        {
            public static FinalBucket Create(IConfirmation job, IActorRef target)
            {
                return new FinalBucket(message: job, target: target);
            }
            private FinalBucket(IConfirmation message, IActorRef target) : base(message: message, target: target)
            {
            }
            public IConfirmation GetObjectFromMessage { get => Message as IConfirmation; }
        }

        public record Break : HiveMessage
        {
            public static Break Create()
            {
                return new Break();
            }
            private Break() : base(null, ActorRefs.NoSender)
            {
            }
        }

        public record FinishSetup : HiveMessage
        {
            public static FinishSetup Create(IActorRef message, IActorRef target)
            {
                return new FinishSetup(message: message, target: target);
            }
            private FinishSetup(IActorRef message, IActorRef target) : base(message: message, target: target)
            {
            }
            public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
        }

        public record UpdateCustomerDueTimes : HiveMessage
        {
            public static UpdateCustomerDueTimes Create(long message, IActorRef target)
            {
                return new UpdateCustomerDueTimes(message: message, target: target);
            }
            private UpdateCustomerDueTimes(object message, IActorRef target) : base(message: message, target: target)
            {

            }
            public long GetObjectFromMessage => (long)Message;
        }
       
    }
}
