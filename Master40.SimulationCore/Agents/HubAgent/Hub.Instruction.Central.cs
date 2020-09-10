using Akka.Actor;
using AkkaSim.Definitions;
using static FCentralActivities;
using static FCentralResourceRegistrations;

namespace Master40.SimulationCore.Agents.HubAgent
{
    public partial class Hub
    {
        /// <summary>
        /// Add instructions for new behaviour in separate class
        /// </summary>
        public partial class Instruction
        {

            public class Central
            {
                public class LoadProductionOrders : SimulationMessage
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

                public class AddResourceToHub : SimulationMessage
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
                public class TryStartActivity : SimulationMessage
                {
                    public static TryStartActivity Create(FCentralActivity message, IActorRef target, bool logThis = false)
                    {
                        return new TryStartActivity(message: message, target: target, logThis: logThis);
                    }

                    private TryStartActivity(object message, IActorRef target, bool logThis) : base(
                        message: message, target: target, logThis: logThis)
                    {

                    }

                    public FCentralActivity GetActivity => Message as FCentralActivity;
                }

            }   
        }
    }
}