using Akka.Actor;
using Master40.SimulationCore.Helper;
using Master40.DB.Nominal;
using Master40.SimulationCore.Agents.HubAgent.Types;
using Master40.SimulationCore.Helper.DistributionProvider;

namespace Master40.SimulationCore.Agents.HubAgent
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class Hub : Agent
    {
        // public Constructor

        public static Props Props(ActorPaths actorPaths, long time, SimulationType simtype, long maxBucketSize, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Hub(actorPaths, time, simtype, maxBucketSize, workTimeGenerator, debug, principal));
        }

        public Hub(ActorPaths actorPaths, long time, SimulationType simtype, long maxBucketSize, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            this.Do(o: BasicInstruction.Initialize.Create(target: Self, message: HubAgent.Behaviour.Factory.Get(simType:simtype, maxBucketSize: maxBucketSize, workTimeGenerator: workTimeGenerator)));
        }

        public static Props Props(ActorPaths actorPaths, long time, SimulationType simtype, long maxBucketSize, string dbConnectionStringGanttPlan, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Hub(actorPaths, time, simtype, maxBucketSize, dbConnectionStringGanttPlan, workTimeGenerator, debug, principal));
        }

        public Hub(ActorPaths actorPaths, long time, SimulationType simtype, long maxBucketSize, string dbConnectionStringGanttPlan, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            this.Do(o: BasicInstruction.Initialize.Create(target: Self, message: HubAgent.Behaviour.Factory.Central(dbConnectionStringGanttPlan, workTimeGenerator: workTimeGenerator)));
        }

        protected override void Finish()
        {
            // Do not Close agent by Finishmessage from Job
        }

    }
}