using Akka.Actor;
using AkkaSim.Definitions;
using static FAgentInformations;
using static IJobs;

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
                    return new SetHubAgent(message: message, target: target);
                }
                private SetHubAgent(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
            }

            public class RequestProposal : SimulationMessage
            {
                public static RequestProposal Create(IJob message, IActorRef target)
                {
                    return new RequestProposal(message: message, target: target);
                }
                private RequestProposal(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public IJob GetObjectFromMessage { get => Message as IJob; }
            }

            public class AcknowledgeProposal : SimulationMessage
            {
                public static AcknowledgeProposal Create(IJob message, IActorRef target)
                {
                    return new AcknowledgeProposal(message: message, target: target);
                }
                private AcknowledgeProposal(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public IJob GetObjectFromMessage { get => Message as IJob; }
            }


            public class DoWork : SimulationMessage
            {
                public static DoWork Create(object message, IActorRef target)
                {
                    return new DoWork(message: message, target: target);
                }
                private DoWork(object message, IActorRef target) : base(message: message, target: target)
                {
                }
            }

        }
    }
}