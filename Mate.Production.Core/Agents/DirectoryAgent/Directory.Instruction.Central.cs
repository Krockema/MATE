using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment.Records.Central;


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
                public static ForwardInsertMaterial Create(StockPostingRecord stockPosting, IActorRef target)
                {
                    return new ForwardInsertMaterial(message: stockPosting, target: target);
                }
                private ForwardInsertMaterial(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public StockPostingRecord GetObjectFromMessage { get => Message as StockPostingRecord; }
            }
            public record ForwardWithdrawMaterial : HiveMessage
            {
                public static ForwardWithdrawMaterial Create(StockPostingRecord stockPosting, IActorRef target)
                {
                    return new ForwardWithdrawMaterial(message: stockPosting, target: target);
                }
                private ForwardWithdrawMaterial(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public StockPostingRecord GetObjectFromMessage { get => Message as StockPostingRecord; }
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
                public static CreateMachineAgents Create(CentralResourceDefinitionRecord message, IActorRef target)
                {
                    return new CreateMachineAgents(message: message, target: target);
                }
                private CreateMachineAgents(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public CentralResourceDefinitionRecord GetObjectFromMessage { get => Message as CentralResourceDefinitionRecord; }
            }
            public record CreateStorageAgents : HiveMessage
            {
                public static CreateStorageAgents Create(CentralStockDefinitionRecord message, IActorRef target)
                {
                    return new CreateStorageAgents(message: message, target: target);
                }
                private CreateStorageAgents(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public CentralStockDefinitionRecord GetObjectFromMessage { get => Message as CentralStockDefinitionRecord; }
            }

            public record ForwardRegistrationToHub : HiveMessage
            {
                public static ForwardRegistrationToHub Create(CentralResourceRegistrationRecord message, IActorRef target)
                {
                    return new ForwardRegistrationToHub(message: message, target: target);
                }
                private ForwardRegistrationToHub(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public CentralResourceRegistrationRecord GetResourceRegistration { get => Message as CentralResourceRegistrationRecord; }
            }

            public record CreateHubAgent : HiveMessage
            {
                public static CreateHubAgent Create(ResourceHubInformationRecord message, IActorRef target)
                {
                    return new CreateHubAgent(message: message, target: target);
                }
                private CreateHubAgent(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public ResourceHubInformationRecord GetObjectFromMessage { get => Message as ResourceHubInformationRecord; }
            }

            public record ForwardAddOrder : HiveMessage
            {
                public static ForwardAddOrder Create(ArticleRecord order, IActorRef target)
                {
                    return new ForwardAddOrder(message: order, target: target);
                }
                private ForwardAddOrder(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public ArticleRecord GetObjectFromMessage { get => Message as ArticleRecord; }
            }

            public record ForwardProvideOrder : HiveMessage
            {
                public static ForwardProvideOrder Create(CentralProvideOrderRecord order, IActorRef target)
                {
                    return new ForwardProvideOrder(message: order, target: target);
                }
                private ForwardProvideOrder(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public CentralProvideOrderRecord GetObjectFromMessage { get => Message as CentralProvideOrderRecord; }
            }

            }
        }
    }
}
