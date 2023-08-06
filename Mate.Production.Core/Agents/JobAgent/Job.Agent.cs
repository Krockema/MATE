using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;
using static IConfirmations;

namespace Mate.Production.Core.Agents.JobAgent
{
    public partial class Job : Agent, IAgent
    {
        IActorRef IAgent.Guardian => ActorRefs.NoSender;

        public static Props Props(ActorPaths actorPaths, IHiveConfig hiveConfig, Configuration configuration, IConfirmation jobConfirmation,Time time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Job(actorPaths, configuration, hiveConfig, jobConfirmation, time, debug, principal));
        }
        public Job(ActorPaths actorPaths, Configuration configuration, IHiveConfig hiveConfig, IConfirmation jobConfirmation, Time time, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time: time, debug: debug, principal: principal)
        {
            DebugMessage(msg: "I'm Alive: " + Context.Self.Path);
            this.Do(BasicInstruction.Initialize.Create(Self, JobAgent.Behaviour.Factory.Get(SimulationType.None, jobConfirmation)));
        }
    }
}
