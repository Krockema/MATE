using Akka.Actor;
using Akka.Hive.Definitions;
using static FCentralActivities;
using static FCentralResourceRegistrations;
using static FCentralStockPostings;

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
                    public static ProvideStorageAgent Create(FCentralStockPosting stockPosting, IActorRef target)
                    {
                        return new ProvideStorageAgent(message: stockPosting, target: target);
                    }
                    private ProvideStorageAgent(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralStockPosting GetObjectFromMessage { get => Message as FCentralStockPosting; }
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
                    public static ActivityFinish Create(FCentralActivity activity, IActorRef target)
                    {
                        return new ActivityFinish(message: activity, target: target);
                    }
                    private ActivityFinish(object message, IActorRef target) : base(message: message, target: target)
                    {
                    }
                    public FCentralActivity GetObjectFromMessage => Message as FCentralActivity;
                }


                public record AddResourceToHub : HiveMessage
                {
                    public static AddResourceToHub Create(FCentralResourceRegistration message, IActorRef target, bool logThis = false)
                    {
                        return new AddResourceToHub(message: message, target: target, logThis: logThis);
                    }

                    private AddResourceToHub(object message, IActorRef target, bool logThis) : base(
                        message: message, target: target, logThis: logThis)
                    {

                    }

                    public FCentralResourceRegistration GetResourceRegistration => Message as FCentralResourceRegistration;
                }
                public record ScheduleActivity : HiveMessage
                {
                    public static ScheduleActivity Create(FCentralActivity message, IActorRef target, bool logThis = false)
                    {
                        return new ScheduleActivity(message: message, target: target, logThis: logThis);
                    }

                    private ScheduleActivity(object message, IActorRef target, bool logThis) : base(
                        message: message, target: target, logThis: logThis)
                    {

                    }

                    public FCentralActivity GetActivity => Message as FCentralActivity;
                }

            }   
        }
    }
}