using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.DataModel;
using System.Collections.Generic;
using static FCapabilityProviderDefinitions;
using static FCentralResourceDefinitions;
using static FCentralResourceHubInformations;
using static FCentralResourceRegistrations;
using static FCentralStockDefinitions;
using static FResourceInformations;

namespace Master40.SimulationCore.Agents.DirectoryAgent
{
    public partial class Directory
    {
        public partial class Instruction
        {
            public class Central
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
                public static CreateMachineAgents Create(FCentralResourceDefinition message, IActorRef target)
                {
                    return new CreateMachineAgents(message: message, target: target);
                }
                private CreateMachineAgents(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FCentralResourceDefinition GetObjectFromMessage { get => Message as FCentralResourceDefinition; }
            }
            public class CreateStorageAgents : SimulationMessage
            {
                public static CreateStorageAgents Create(FCentralStockDefinition message, IActorRef target)
                {
                    return new CreateStorageAgents(message: message, target: target);
                }
                private CreateStorageAgents(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FCentralStockDefinition GetObjectFromMessage { get => Message as FCentralStockDefinition; }
            }

            public class ForwardRegistrationToHub : SimulationMessage
            {
                public static ForwardRegistrationToHub Create(FCentralResourceRegistration message, IActorRef target)
                {
                    return new ForwardRegistrationToHub(message: message, target: target);
                }
                private ForwardRegistrationToHub(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FCentralResourceRegistration GetResourceRegistration { get => Message as FCentralResourceRegistration; }
            }

            public class CreateHubAgent : SimulationMessage
            {
                public static CreateHubAgent Create(FResourceHubInformation message, IActorRef target)
                {
                    return new CreateHubAgent(message: message, target: target);
                }
                private CreateHubAgent(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FResourceHubInformation GetObjectFromMessage { get => Message as FResourceHubInformation; }
            }

            }
        }
    }
}
