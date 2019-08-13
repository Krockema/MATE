using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationCore.Types;
using static FBreakDowns;
using static FHubInformations;

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


        public class ResponseFromHub : SimulationMessage
        {
            public static ResponseFromHub Create(FHubInformation message, IActorRef target)
            {
                return new ResponseFromHub(message, target);
            }
            private ResponseFromHub(object message, IActorRef target) : base(message, target)
            {
            }
            public FHubInformation GetObjectFromMessage { get => Message as FHubInformation; }
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
