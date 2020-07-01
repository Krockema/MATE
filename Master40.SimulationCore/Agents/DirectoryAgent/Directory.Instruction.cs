using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.DataModel;
using System.Collections.Generic;
using static FCapabilityProviderDefinitions;
using static FResourceHubInformations;
using static FResourceInformations;

namespace Master40.SimulationCore.Agents.DirectoryAgent
{
    public partial class Directory
    {
        public class Instruction
        {
            public class RequestAgent : SimulationMessage
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

            public class RegisterResources : SimulationMessage
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

            public class CreateMachineAgents : SimulationMessage
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
            public class CreateStorageAgents : SimulationMessage
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

            public class ForwardRegistrationToHub : SimulationMessage
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

            public class CreateResourceHubAgents : SimulationMessage
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
