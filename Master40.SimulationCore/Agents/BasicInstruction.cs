using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationCore.Types;
using static FBreakDowns;
using static FAgentInformations;
using static FArticles;

namespace Master40.SimulationCore.Agents
{
    public class BasicInstruction
    {
        public class Initialize : SimulationMessage
        {
            public static Initialize Create(IActorRef target, IBehaviour message = null)
            {
                return new Initialize(message, target);
            }
            private Initialize(object message, IActorRef target) : base(message, target)
            {
            }
            public IBehaviour GetObjectFromMessage { get => Message as IBehaviour; }
        }

        public class ChildRef : SimulationMessage
        {
            public static ChildRef Create(IActorRef message, IActorRef target)
            {
                return new ChildRef(message, target);
            }
            private ChildRef(object message, IActorRef target) : base(message, target)
            {
            }
            public IActorRef GetObjectFromMessage { get => Message as IActorRef; }
        }


        public class ResponseFromDirectory : SimulationMessage
        {
            public static ResponseFromDirectory Create(FAgentInformation message, IActorRef target)
            {
                return new ResponseFromDirectory(message, target);
            }
            private ResponseFromDirectory(object message, IActorRef target) : base(message, target)
            {
            }
            public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
        }

        public class ProvideArticle : SimulationMessage
        {
            public static ProvideArticle Create(FArticle message, IActorRef target, bool logThis)
            {
                return new ProvideArticle(message, target, logThis);
            }
            private ProvideArticle(object message, IActorRef target, bool logThis) : base(message, target, logThis)
            {

            }
            public FArticle GetObjectFromMessage { get => Message as FArticle; }
        }

        public class ResourceBrakeDown : SimulationMessage
        {
            public static ResourceBrakeDown Create(FBreakDown message, IActorRef target, bool logThis = false)
            {
                return new ResourceBrakeDown(message, target, logThis);
            }
            private ResourceBrakeDown(object message, IActorRef target, bool logThis) : base(message, target, logThis)
            {
            }
            public FBreakDown GetObjectFromMessage { get => Message as FBreakDown; }
        }

        public class RemoveVirtualChild : SimulationMessage
        {
            public static RemoveVirtualChild Create(IActorRef target, bool logThis = false)
            {
                return new RemoveVirtualChild(null, target, logThis);
            }
            private RemoveVirtualChild(object message, IActorRef target, bool logThis) : base(message, target, logThis)
            {
            }
        }
    }
}
