using System;
using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationCore.Types;
using static FBreakDowns;
using static FAgentInformations;
using static FArticles;
using static FArticleProviders;
using static IJobResults;
using static FUpdateStartConditions;

namespace Master40.SimulationCore.Agents
{
    public class BasicInstruction
    {
        public class Initialize : SimulationMessage
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

        public class ChildRef : SimulationMessage
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
        public class JobForwardEnd : SimulationMessage
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

        public class ResponseFromDirectory : SimulationMessage
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

        public class ProvideArticle : SimulationMessage
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

        public class ResourceBrakeDown : SimulationMessage
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

        public class RemoveVirtualChild : SimulationMessage
        {
            public static RemoveVirtualChild Create(IActorRef target, bool logThis = false)
            {
                return new RemoveVirtualChild(message: null, target: target, logThis: logThis);
            }
            private RemoveVirtualChild(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
            {
            }
        }

        public class WithdrawRequiredArticles : SimulationMessage
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

        public class FinishJob : SimulationMessage
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
        public class UpdateStartConditions : SimulationMessage
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

    }
}
