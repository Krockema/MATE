using Akka.Actor;
using AkkaSim.Definitions;

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
            }   
        }
    }
}