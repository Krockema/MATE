using Akka.Actor;
using AkkaSim.Definitions;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.Ressource
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
                public static RequestProposal Create(FBucket message, IActorRef target)
                {
                    return new RequestProposal(message, target);
                }
                private RequestProposal(object message, IActorRef target) : base(message, target)
                {
                }
                public FBucket GetObjectFromMessage { get => Message as FBucket; }
            }

            public class AcknowledgeProposal : SimulationMessage
            {
                public static AcknowledgeProposal Create(FBucket message, IActorRef target)
                {
                    return new AcknowledgeProposal(message, target);
                }
                private AcknowledgeProposal(object message, IActorRef target) : base(message, target)
                {
                }
                public FBucket GetObjectFromMessage { get => Message as FBucket; }
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
                public static FinishWork Create(FBucket message, IActorRef target)
                {
                    return new FinishWork(message, target);
                }
                private FinishWork(object message, IActorRef target) : base(message, target)
                {
                }
                public FBucket GetObjectFromMessage { get => Message as FBucket; }
            }
        }
    }
}
