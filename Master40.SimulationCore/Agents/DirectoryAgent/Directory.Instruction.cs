using Akka.Actor;
using AkkaSim.Definitions;
using Master40.DB.DataModel;
using static FAgentInformations;
using static FResourceSetupDefinitions;

namespace Master40.SimulationCore.Agents.DirectoryAgent
{
    public partial class Directory
    {
        public class Instruction
        {
            public class CreateHubAgent : SimulationMessage
            {
                public static CreateHubAgent Create(FAgentInformation message, IActorRef target)
                {
                    return new CreateHubAgent(message, target);
                }
                private CreateHubAgent(object message, IActorRef target) : base(message, target)
                {
                }
                public FAgentInformation GetObjectFromMessage { get => Message as FAgentInformation; }
            }

            public class RequestAgent : SimulationMessage
            {
                public static RequestAgent Create(string descriminator, IActorRef target)
                {
                    return new RequestAgent(descriminator, target);
                }
                private RequestAgent(object message, IActorRef target) : base(message, target)
                {
                }
                public string GetObjectFromMessage { get => Message as string; }
            }

            public class RegisterResources : SimulationMessage
            {
                public static RegisterResources Create(string descriminator, IActorRef target)
                {
                    return new RegisterResources(descriminator, target);
                }
                private RegisterResources(object message, IActorRef target) : base(message, target)
                {
                }
                public string GetObjectFromMessage { get => Message as string; }
            }

            public class CreateMachineAgents : SimulationMessage
            {
                public static CreateMachineAgents Create(FResourceSetupDefinition message, IActorRef target)
                {
                    return new CreateMachineAgents(message, target);
                }
                private CreateMachineAgents(object message, IActorRef target) : base(message, target)
                {
                }
                public FResourceSetupDefinition GetObjectFromMessage { get => Message as FResourceSetupDefinition; }
            }
            public class CreateStorageAgents : SimulationMessage
            {
                public static CreateStorageAgents Create(M_Stock message, IActorRef target)
                {
                    return new CreateStorageAgents(message, target);
                }
                private CreateStorageAgents(object message, IActorRef target) : base(message, target)
                {
                }
                public M_Stock GetObjectFromMessage { get => Message as M_Stock; }
            }
        }
    }
}
