using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment.Records.Central;

namespace Mate.Production.Core.Agents.HubAgent
{
    public partial class Hub
    {
        /// <summary>
        /// Add instructions for new behaviour in separate class
        /// </summary>
        public partial class Instruction
        {

            public record Central
            {
                public record LoadProductionOrders : HiveMessage
                {
                    public static LoadProductionOrders Create(IActorRef inbox, IActorRef target, bool logThis = false)
                    {
                        return new LoadProductionOrders(message: inbox, target: target, logThis: logThis);
                    }

                    private LoadProductionOrders(object message, IActorRef target, bool logThis) : base(
                        message: message, target: target, logThis: logThis)
                    {

                    }

                    public IActorRef GetInboxActorRef => (IActorRef)Message;
                }

                public record ProvideStorageAgent : HiveMessage
                {
                    public static ProvideStorageAgent Create(StockPostingRecord stockPosting, IActorRef target)
                    {
                        return new ProvideStorageAgent(message: stockPosting, target: target);
                    }
                    private ProvideStorageAgent(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public StockPostingRecord GetObjectFromMessage { get => Message as StockPostingRecord; }
                }
                
                public record StartActivities : HiveMessage
                {
                    public static StartActivities Create(IActorRef target, bool logThis = false)
                    {
                        return new StartActivities(message: null ,target: target, logThis: logThis);
                    }

                    private StartActivities(object message, IActorRef target, bool logThis) : base(
                        message: message, target: target, logThis: logThis)
                    {

                    }

                }

                public record ActivityFinish : HiveMessage
                {
                    public static ActivityFinish Create(CentralActivityRecord activity, IActorRef target)
                    {
                        return new ActivityFinish(message: activity, target: target);
                    }
                    private ActivityFinish(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public CentralActivityRecord GetObjectFromMessage => Message as CentralActivityRecord;
                }


                public record AddResourceToHub : HiveMessage
                {
                    public static AddResourceToHub Create(CentralResourceRegistrationRecord message, IActorRef target, bool logThis = false)
                    {
                        return new AddResourceToHub(message: message, target: target, logThis: logThis);
                    }

                    private AddResourceToHub(object message, IActorRef target, bool logThis) : base(
                        message: message, target: target, logThis: logThis)
                    {

                    }

                    public CentralResourceRegistrationRecord GetResourceRegistration => Message as CentralResourceRegistrationRecord;
                }
                public record ScheduleActivity : HiveMessage
                {
                    public static ScheduleActivity Create(CentralActivityRecord message, IActorRef target, bool logThis = false)
                    {
                        return new ScheduleActivity(message: message, target: target, logThis: logThis);
                    }

                    private ScheduleActivity(object message, IActorRef target, bool logThis) : base(
                        message: message, target: target, logThis: logThis)
                    {

                    }

                    public CentralActivityRecord GetActivity => Message as CentralActivityRecord;
                }

            }   
        }
    }
}