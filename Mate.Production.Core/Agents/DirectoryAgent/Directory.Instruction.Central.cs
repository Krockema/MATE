using Akka.Actor;
using Akka.Hive.Definitions;
using static FArticles;
using static FCentralProvideOrders;
using static FCentralResourceDefinitions;
using static FCentralResourceHubInformations;
using static FCentralResourceRegistrations;
using static FCentralStockDefinitions;
using static FCentralStockPostings;

namespace Mate.Production.Core.Agents.DirectoryAgent
{
    public partial class Directory
    {
        public partial class Instruction
        {
            public record Central
            {

            public record ForwardInsertMaterial : HiveMessage
            {
                public static ForwardInsertMaterial Create(FCentralStockPosting stockPosting, IActorRef target)
                {
                    return new ForwardInsertMaterial(message: stockPosting, target: target);
                }
                private ForwardInsertMaterial(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FCentralStockPosting GetObjectFromMessage { get => Message as FCentralStockPosting; }
            }
            public record ForwardWithdrawMaterial : HiveMessage
            {
                public static ForwardWithdrawMaterial Create(FCentralStockPosting stockPosting, IActorRef target)
                {
                    return new ForwardWithdrawMaterial(message: stockPosting, target: target);
                }
                private ForwardWithdrawMaterial(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FCentralStockPosting GetObjectFromMessage { get => Message as FCentralStockPosting; }
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
                public static CreateMachineAgents Create(FCentralResourceDefinition message, IActorRef target)
                {
                    return new CreateMachineAgents(message: message, target: target);
                }
                private CreateMachineAgents(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FCentralResourceDefinition GetObjectFromMessage { get => Message as FCentralResourceDefinition; }
            }
            public record CreateStorageAgents : HiveMessage
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

            public record ForwardRegistrationToHub : HiveMessage
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

            public record CreateHubAgent : HiveMessage
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

            public record ForwardAddOrder : HiveMessage
            {
                public static ForwardAddOrder Create(FArticle order, IActorRef target)
                {
                    return new ForwardAddOrder(message: order, target: target);
                }
                private ForwardAddOrder(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FArticle GetObjectFromMessage { get => Message as FArticle; }
            }

            public record ForwardProvideOrder : HiveMessage
            {
                public static ForwardProvideOrder Create(FCentralProvideOrder order, IActorRef target)
                {
                    return new ForwardProvideOrder(message: order, target: target);
                }
                private ForwardProvideOrder(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public FCentralProvideOrder GetObjectFromMessage { get => Message as FCentralProvideOrder; }
            }

            }
        }
    }
}
