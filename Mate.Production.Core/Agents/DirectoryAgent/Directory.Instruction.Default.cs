using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using static FCapabilityProviderDefinitions;
using static FResourceHubInformations;
using static FResourceInformations;

namespace Mate.Production.Core.Agents.DirectoryAgent
{
    public partial class Directory
    {
        public partial class Instruction
        {
            public record Default
            {

            public record RequestAgent : HiveMessage
            {
                public static RequestAgent Create(string discriminator, IActorRef target)
                {
                    return new RequestAgent(message: discriminator, target: target);
                }
                private RequestAgent(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public string GetObjectFromMessage { get => Message as string; }
            }

            public record RegisterResources : HiveMessage
            {
                public static RegisterResources Create(string descriminator, IActorRef target)
                {
                    return new RegisterResources(message: descriminator, target: target);
                }
                private RegisterResources(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public string GetObjectFromMessage { get => Message as string; }
            }

            public record CreateMachineAgents : HiveMessage
            {
                public static CreateMachineAgents Create(FCapabilityProviderDefinition message, IActorRef target)
                {
                    return new CreateMachineAgents(message: message, target: target);
                }
                private CreateMachineAgents(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FCapabilityProviderDefinition GetObjectFromMessage { get => Message as FCapabilityProviderDefinition; }
            }
            public record CreateStorageAgents : HiveMessage
            {
                public static CreateStorageAgents Create(M_Stock message, IActorRef target)
                {
                    return new CreateStorageAgents(message: message, target: target);
                }
                private CreateStorageAgents(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public M_Stock GetObjectFromMessage { get => Message as M_Stock; }
            }

            public record ForwardRegistrationToHub : HiveMessage
            {
                public static ForwardRegistrationToHub Create(FResourceInformation message, IActorRef target)
                {
                    return new ForwardRegistrationToHub(message: message, target: target);
                }
                private ForwardRegistrationToHub(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FResourceInformation GetObjectFromMessage { get => Message as FResourceInformation; }
            }

            public record CreateResourceHubAgents : HiveMessage
            {
                public static CreateResourceHubAgents Create(FResourceHubInformation message, IActorRef target)
                {
                    return new CreateResourceHubAgents(message: message, target: target);
                }
                private CreateResourceHubAgents(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FResourceHubInformation GetObjectFromMessage { get => Message as FResourceHubInformation; }
            }

            }
        }
    }
}
