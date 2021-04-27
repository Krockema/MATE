using Akka.Actor;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;

namespace Mate.Production.Core.Agents.HubAgent
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class Hub : Agent
    {
        // public Constructor

        public static Props Props(ActorPaths actorPaths, Configuration configuration, long time, SimulationType simtype, long maxBucketSize, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Hub(actorPaths, configuration,  time, simtype, maxBucketSize, workTimeGenerator, debug, principal));
        }

        public Hub(ActorPaths actorPaths, Configuration configuration, long time, SimulationType simtype, long maxBucketSize, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, time: time, debug: debug, principal: principal)
        {
            this.Do(o: BasicInstruction.Initialize.Create(target: Self, message: HubAgent.Behaviour.Factory.Get(simType:simtype, maxBucketSize: maxBucketSize, workTimeGenerator: workTimeGenerator)));
        }

        public static Props Props(ActorPaths actorPaths, Configuration configuration, long time, SimulationType simtype, long maxBucketSize, string dbConnectionStringGanttPlan, string dbConnectionStringMaster, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Hub(actorPaths, configuration, time, simtype, maxBucketSize, dbConnectionStringGanttPlan, dbConnectionStringMaster, workTimeGenerator, debug, principal));
        }

        public Hub(ActorPaths actorPaths, Configuration configuration, long time, SimulationType simtype, long maxBucketSize, string dbConnectionStringGanttPlan, string dbConnectionStringMaster, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, time, debug: debug, principal: principal)
        {
            this.Do(o: BasicInstruction.Initialize.Create(target: Self, message: HubAgent.Behaviour.Factory.Central(dbConnectionStringGanttPlan, dbConnectionStringMaster, workTimeGenerator: workTimeGenerator)));
        }

        protected override void Finish()
        {
            // Do not Close agent by Finishmessage from Job
        }

    }
}