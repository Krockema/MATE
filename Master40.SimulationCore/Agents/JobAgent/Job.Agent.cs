using Akka.Actor;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Helper;
using System.Linq;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;

namespace Master40.SimulationCore.Agents.JobAgent
{
    /// <summary>
    /// --------- General sequence
    /// </summary>

    public partial class Job : Agent, IAgent
    {

        IActorRef IAgent.Guardian => ActorRefs.NoSender;

        // public Constructor
        public static Props Props(ActorPaths actorPaths, JobConfirmation jobConfirmation,long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Job(actorPaths, jobConfirmation, time, debug, principal));
        }
        public Job(ActorPaths actorPaths, JobConfirmation jobConfirmation, long time, bool debug, IActorRef principal) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            DebugMessage(msg: "I'm Alive: " + Context.Self.Path);
            this.Do(BasicInstruction.Initialize.Create(Self, JobAgent.Behaviour.Factory.Get(SimulationType.None, jobConfirmation)));
        }

        protected override void OnChildAdd(IActorRef childRef)
        {
        }
    }
}
