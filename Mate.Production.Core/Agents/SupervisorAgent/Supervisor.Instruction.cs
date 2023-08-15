using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records;

namespace Mate.Production.Core.Agents.SupervisorAgent
{
    public partial class Supervisor
    {
        public class Instruction
        {
            /*
                CreateContractAgent,
                RequestArticleBom,
                OrderProvided,
            */
            public record CreateContractAgent : HiveMessage
            {
                public static CreateContractAgent Create(T_CustomerOrder message, IActorRef target)
                {
                    return new CreateContractAgent(message: message, target: target);
                }
                private CreateContractAgent(T_CustomerOrder message, IActorRef target) : base(message: message, target: target)
                {
                }
                public T_CustomerOrder GetObjectFromMessage { get => Message as T_CustomerOrder; }

            }
            public record RequestArticleBom : HiveMessage
            {
                public static RequestArticleBom Create(int message, IActorRef target)
                {
                    return new RequestArticleBom(message: message, target: target);
                }
                private RequestArticleBom(object message, IActorRef target) : base(message: message, target: target)
                {
                }
                public int GetObjectFromMessage { get => (int)Message; }
            }
            public record OrderProvided : HiveMessage
            {
                public static OrderProvided Create(ArticleRecord message, IActorRef target)
                {
                    return new OrderProvided(message: message, target: target);
                }
                private OrderProvided(object message, IActorRef target) : base(message: message, target: target, logThis: true)
                {
                }
                public ArticleRecord GetObjectFromMessage { get => Message as ArticleRecord; }
            }

            public record EndSimulation : HiveMessage
            {
                public static EndSimulation Create(object message, IActorRef target)
                {
                    return new EndSimulation(message: message, target: target);
                }
                private EndSimulation(object message, IActorRef target) : base(message: message, target: target)
                {
                }
            }

            public record PopOrder : HiveMessage
            {
                public static PopOrder Create(string message, IActorRef target)
                {
                    return new PopOrder(message: message, target: target);
                }
                private PopOrder(object message, IActorRef target) : base(message: message, target: target)
                {
                }
            }

            public record SystemCheck : HiveMessage
            {
                public static SystemCheck Create(string message, IActorRef target)
                {
                    return new SystemCheck(message: message, target: target);
                }
                private SystemCheck(object message, IActorRef target) : base(message: message, target: target)
                {
                }
            }

            public record SetEstimatedThroughputTime : HiveMessage
            {
                public static SetEstimatedThroughputTime Create(SetEstimatedThroughputTimeRecord message, IActorRef target)
                {
                    return new SetEstimatedThroughputTime(message: message, target: target);
                }
                private SetEstimatedThroughputTime(object message, IActorRef target) : base(message: message, target: target)
                {

                }
                public SetEstimatedThroughputTimeRecord GetObjectFromMessage { get => Message as SetEstimatedThroughputTimeRecord; }
            }
        }
    }
}
