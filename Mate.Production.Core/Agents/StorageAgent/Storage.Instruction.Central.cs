using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment.Records.Central;

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
                    public static AddOrder Create(ArticleRecord message, IActorRef target)
                    {
                        return new AddOrder(message: message, target: target);
                    }
                    private AddOrder(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public ArticleRecord GetObjectFromMessage { get => Message as ArticleRecord; }
                }

                public record ProvideOrderAtDue : HiveMessage
                {
                    public static ProvideOrderAtDue Create(CentralProvideOrderRecord message, IActorRef target)
                    {
                        return new ProvideOrderAtDue(message: message, target: target);
                    }
                    private ProvideOrderAtDue(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public CentralProvideOrderRecord GetObjectFromMessage { get => Message as CentralProvideOrderRecord; }
                }
                public record WithdrawMaterial : HiveMessage
                {
                    public static WithdrawMaterial Create(StockPostingRecord message, IActorRef target)
                    {
                        return new WithdrawMaterial(message: message, target: target);
                    }
                    private WithdrawMaterial(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public StockPostingRecord GetObjectFromMessage { get => Message as StockPostingRecord; }
                }
                public record InsertMaterial : HiveMessage
                {
                    public static InsertMaterial Create(StockPostingRecord message, IActorRef target)
                    {
                        return new InsertMaterial(message: message, target: target);
                    }
                    private InsertMaterial(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public StockPostingRecord GetObjectFromMessage { get => Message as StockPostingRecord; }
                }

                public record AddPurchase : HiveMessage
                {
                    public static AddPurchase Create(CentralPurchaseRecord message, IActorRef target)
                    {
                        return new AddPurchase(message: message, target: target);
                    }
                    private AddPurchase(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public CentralPurchaseRecord GetObjectFromMessage { get => Message as CentralPurchaseRecord; }

                }
                public record PopPurchase : HiveMessage
                {
                    public static PopPurchase Create(CentralPurchaseRecord message, IActorRef target)
                    {
                        return new PopPurchase(message: message, target: target);
                    }
                    private PopPurchase(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public CentralPurchaseRecord GetObjectFromMessage { get => Message as CentralPurchaseRecord; }
                }

            }
        }
    }
}
