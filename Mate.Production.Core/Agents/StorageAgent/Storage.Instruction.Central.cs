using Akka.Actor;
using AkkaSim.Definitions;
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

            public class Central
            {

                public class AddOrder : SimulationMessage
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

                public class ProvideOrderAtDue : SimulationMessage
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
                public class WithdrawMaterial : SimulationMessage
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
                public class InsertMaterial : SimulationMessage
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

                public class AddPurchase : SimulationMessage
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
                public class PopPurchase : SimulationMessage
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
