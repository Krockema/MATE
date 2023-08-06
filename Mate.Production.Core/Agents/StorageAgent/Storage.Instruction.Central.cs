using Akka.Actor;
using Akka.Hive.Definitions;
using static FArticles;
using static FCentralProvideOrders;
using static FCentralPurchases;
using static FCentralStockPostings;

namespace Mate.Production.Core.Agents.StorageAgent
{
    public partial class Storage
    {
        public partial class Instruction
        {

            public record Central
            {

                public record AddOrder : HiveMessage
                {
                    public static AddOrder Create(FArticle message, IActorRef target)
                    {
                        return new AddOrder(message: message, target: target);
                    }
                    private AddOrder(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FArticle GetObjectFromMessage { get => Message as FArticle; }
                }

                public record ProvideOrderAtDue : HiveMessage
                {
                    public static ProvideOrderAtDue Create(FCentralProvideOrder message, IActorRef target)
                    {
                        return new ProvideOrderAtDue(message: message, target: target);
                    }
                    private ProvideOrderAtDue(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralProvideOrder GetObjectFromMessage { get => Message as FCentralProvideOrder; }
                }
                public record WithdrawMaterial : HiveMessage
                {
                    public static WithdrawMaterial Create(FCentralStockPosting message, IActorRef target)
                    {
                        return new WithdrawMaterial(message: message, target: target);
                    }
                    private WithdrawMaterial(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralStockPosting GetObjectFromMessage { get => Message as FCentralStockPosting; }
                }
                public record InsertMaterial : HiveMessage
                {
                    public static InsertMaterial Create(FCentralStockPosting message, IActorRef target)
                    {
                        return new InsertMaterial(message: message, target: target);
                    }
                    private InsertMaterial(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralStockPosting GetObjectFromMessage { get => Message as FCentralStockPosting; }
                }

                public record AddPurchase : HiveMessage
                {
                    public static AddPurchase Create(FCentralPurchase message, IActorRef target)
                    {
                        return new AddPurchase(message: message, target: target);
                    }
                    private AddPurchase(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralPurchase GetObjectFromMessage { get => Message as FCentralPurchase; }

                }
                public record PopPurchase : HiveMessage
                {
                    public static PopPurchase Create(FCentralPurchase message, IActorRef target)
                    {
                        return new PopPurchase(message: message, target: target);
                    }
                    private PopPurchase(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralPurchase GetObjectFromMessage { get => Message as FCentralPurchase; }
                }

            }
        }
    }
}
