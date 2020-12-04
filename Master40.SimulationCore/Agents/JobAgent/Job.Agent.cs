using Akka.Actor;
using Master40.DB.Nominal;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Environment;
using static IConfirmations;

namespace Master40.SimulationCore.Agents.JobAgent
{
    public partial class Job : Agent, IAgent
    {
        IActorRef IAgent.Guardian => ActorRefs.NoSender;

        public static Props Props(ActorPaths actorPaths, Configuration configuration, IConfirmation jobConfirmation,long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Job(actorPaths, configuration, jobConfirmation, time, debug, principal));
        }
        public Job(ActorPaths actorPaths, Configuration configuration, IConfirmation jobConfirmation, long time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: principal)
        {
            DebugMessage(msg: "I'm Alive: " + Context.Self.Path);
            this.Do(BasicInstruction.Initialize.Create(Self, JobAgent.Behaviour.Factory.Get(SimulationType.None, jobConfirmation)));
        }
    }
}
