using Akka.Actor;
using AkkaSim.Definitions;
using static FAgentInformations;
using static FBucketToRequeues;
using static IJobResults;
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

            public class FinishBucket : SimulationMessage
            {
                public static FinishBucket Create(IJobResult message, IActorRef target)
                {
                    return new FinishBucket(message: message, target: target);
                }
                private FinishBucket(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public IJobResult GetObjectFromMessage { get => Message as IJobResult; }
            }

            public class AskRequeueBucket : SimulationMessage
            {
                public static AskRequeueBucket Create(FBucketToRequeue message, IActorRef target)
                {
                    return new AskRequeueBucket(message: message, target: target);
                }
                private AskRequeueBucket(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FBucketToRequeue GetObjectFromMessage { get => Message as FBucketToRequeue; }
            }

            
        }
    }
}