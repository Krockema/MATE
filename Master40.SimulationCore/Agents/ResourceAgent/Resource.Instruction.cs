using Akka.Actor;
using AkkaSim.Definitions;
using System;
using static FAgentInformations;
using static FOperations;
using static IJobs;
using static FOperationResults;

namespace Master40.SimulationCore.Agents.ResourceAgent
{
    public partial class Resource
    {
        public class Instruction
        {
            public class SetHubAgent : SimulationMessage
            {
                public static SetHubAgent Create(FAgentInformation message, IActorRef target)
                {
                    return new SetHubAgent(message, target);
                }
                private SetHubAgent(object message, IActorRef target) : base(message, target)
                {
                }
                public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
            }

            public class RequestProposal : SimulationMessage
            {
                public static RequestProposal Create(IJob message, IActorRef target)
                {
                    return new RequestProposal(message, target);
                }
                private RequestProposal(object message, IActorRef target) : base(message, target)
                {
                }
                public IJob GetObjectFromMessage { get => Message as IJob; }
            }

            public class AcknowledgeProposal : SimulationMessage
            {
                public static AcknowledgeProposal Create(FOperation message, IActorRef target)
                {
                    return new AcknowledgeProposal(message, target);
                }
                private AcknowledgeProposal(object message, IActorRef target) : base(message, target)
                {
                }
                public FOperation GetObjectFromMessage { get => Message as FOperation; }
            }

            public class StartWorkWith : SimulationMessage
            {
                public static StartWorkWith Create(Guid message, IActorRef target)
                {
                    return new StartWorkWith(message, target);
                }
                private StartWorkWith(object message, IActorRef target) : base(message, target)
                {
                }
                public Guid GetObjectFromMessage { get => (Guid)Message; }
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
                public static FinishWork Create(FOperationResult message, IActorRef target)
                {
                    return new FinishWork(message, target);
                }
                private FinishWork(object message, IActorRef target) : base(message, target)
                {
                }
                public FOperationResult GetObjectFromMessage { get => Message as FOperationResult; }
            }
        }
    }
}