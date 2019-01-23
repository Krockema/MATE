using Akka.Actor;
using AkkaSim.Definitions;
using AkkaSim.Interfaces;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Text;

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
            public static ResponseFromHub Create(HubInformation message, IActorRef target)
            {
                return new ResponseFromHub(message, target);
            }
            private ResponseFromHub(object message, IActorRef target) : base(message, target)
            {
            }
            public HubInformation GetObjectFromMessage { get => Message as HubInformation; }
        }
    }
}
