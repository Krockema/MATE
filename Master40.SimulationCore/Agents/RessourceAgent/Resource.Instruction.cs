using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents
{
    public partial class Resource
    {
        public class Instruction
        {
            public class SetHubAgent : SimulationMessage
            {
                public static SetHubAgent Create(FHubInformation message, IActorRef target)
                {
                    return new SetHubAgent(message, target);
                }
                private SetHubAgent(object message, IActorRef target) : base(message, target)
                {
                }
                public FHubInformation GetObjectFromMessage { get => Message as FHubInformation; }
            }

            public class RequestProposal : SimulationMessage
            {
                public static RequestProposal Create(FWorkItem message, IActorRef target)
                {
                    return new RequestProposal(message, target);
                }
                private RequestProposal(object message, IActorRef target) : base(message, target)
                {
                }
                public FWorkItem GetObjectFromMessage { get => Message as FWorkItem; }
            }

            public class AcknowledgeProposal : SimulationMessage
            {
                public static AcknowledgeProposal Create(FWorkItem message, IActorRef target)
                {
                    return new AcknowledgeProposal(message, target);
                }
                private AcknowledgeProposal(object message, IActorRef target) : base(message, target)
                {
                }
                public FWorkItem GetObjectFromMessage { get => Message as FWorkItem; }
            }

            public class StartWorkWith : SimulationMessage
            {
                public static StartWorkWith Create(FItemStatus message, IActorRef target)
                {
                    return new StartWorkWith(message, target);
                }
                private StartWorkWith(object message, IActorRef target) : base(message, target)
                {
                }
                public FItemStatus GetObjectFromMessage { get => Message as FItemStatus; }
            }

            public class DoWork : SimulationMessage
            {
                public static DoWork Create(object message, IActorRef target)
                {
                    return new DoWork(message, target);
                }
                private DoWork(object message, IActorRef target) : base(message, target)
                {
                }
            }

            public class FinishWork : SimulationMessage
            {
                public static FinishWork Create(FWorkItem message, IActorRef target)
                {
                    return new FinishWork(message, target);
                }
                private FinishWork(object message, IActorRef target) : base(message, target)
                {
                }
                public FWorkItem GetObjectFromMessage { get => Message as FWorkItem; }
            }
        }
    }
}
