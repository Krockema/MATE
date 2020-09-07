using Akka.Actor;
using AkkaSim.Definitions;
using static FProposals;
using static FResourceInformations;
using static IJobs;

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
                public class LoadProducationOrderNet : SimulationMessage
                {
                    public static LoadProducationOrderNet Create(IActorRef target, bool logThis = false)
                    {
                        return new LoadProducationOrderNet(message: null, target: target, logThis: logThis);
                    }
                    private LoadProducationOrderNet(object message, IActorRef target, bool logThis) : base(message: message, target: target, logThis: logThis)
                    {

                    }
                    public object GetObjectFromMessage { get => Message as object; }
                }
                
            }
        }
    }
}