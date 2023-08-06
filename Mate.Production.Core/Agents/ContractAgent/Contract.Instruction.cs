using Akka.Actor;
using Akka.Hive.Definitions;
using Akka.Hive.Interfaces;
using Mate.DataCore.DataModel;

namespace Mate.Production.Core.Agents.ContractAgent
{
    public partial class Contract
    {
        public record Instruction { 
            public record StartOrder : HiveMessage
            {
                private StartOrder(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
                {
                }

                public static IHiveMessage Create(T_CustomerOrderPart message, IActorRef target, bool logThis = false)
                {
                    return new StartOrder(message: message, target: target, logThis: logThis);
                }
                public T_CustomerOrderPart GetObjectFromMessage { get => Message as T_CustomerOrderPart; }
            }

            public record TryFinishOrder : HiveMessage
            {
                private TryFinishOrder(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
                {
                }

                public static IHiveMessage Create(CentralProvideOrderRecord message, IActorRef target, bool logThis = false)
                {
                    return new TryFinishOrder(message: message, target: target, logThis: logThis);
                }
                public CentralProvideOrderRecord GetObjectFromMessage { get => Message as CentralProvideOrderRecord; }
            }

            

            public record Finish : HiveMessage
            {
                private Finish(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
                {
                }

                public static IHiveMessage Create(ArticleRecord requestItem, IActorRef target, bool logThis)
                {
                    return new Finish(message: requestItem, target: target, logThis: logThis);
                }
                public ArticleRecord GetObjectFromMessage { get => Message as ArticleRecord; }

            }
        }
    }
}
