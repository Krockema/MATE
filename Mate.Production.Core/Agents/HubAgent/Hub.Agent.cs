using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;
using System;

namespace Mate.Production.Core.Agents.HubAgent
{
    /// <summary>
    /// Alternative Namen; ResourceAllocation, RessourceGroup, Mediator, Coordinator, Hub
    /// </summary>
    public partial class Hub : Agent
    {
        // public Constructor

        public static Props Props(ActorPaths actorPaths, IHiveConfig hiveConfig, Configuration configuration, Time time, SimulationType simtype, TimeSpan maxBucketSize, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Hub(actorPaths, hiveConfig, configuration, time, simtype, maxBucketSize, workTimeGenerator, debug, principal));
        }

        public Hub(ActorPaths actorPaths, IHiveConfig hiveConfig, Configuration configuration, Time time, SimulationType simtype, TimeSpan maxBucketSize, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time: time, debug: debug, principal: principal)
        {
            this.Do(o: BasicInstruction.Initialize.Create(target: Self, message: HubAgent.Behaviour.Factory.Get(simType:simtype, maxBucketSize: maxBucketSize, workTimeGenerator: workTimeGenerator)));
        }

        public static Props Props(ActorPaths actorPaths, IHiveConfig hiveConfig, Configuration configuration, Time time, SimulationType simtype, TimeSpan maxBucketSize, string dbConnectionStringGanttPlan, string dbConnectionStringMaster, string pathToGANTTPLANOptRunner, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Hub(actorPaths, hiveConfig, configuration, time, simtype, maxBucketSize, dbConnectionStringGanttPlan, dbConnectionStringMaster, pathToGANTTPLANOptRunner, workTimeGenerator, debug, principal));
        }

        public Hub(ActorPaths actorPaths, IHiveConfig hiveConfig, Configuration configuration, Time time, SimulationType simtype, TimeSpan maxBucketSize, string dbConnectionStringGanttPlan, string dbConnectionStringMaster, string pathToGANTTPLANOptRunner, WorkTimeGenerator workTimeGenerator, bool debug, IActorRef principal) 
            : base(actorPaths: actorPaths, configuration: configuration, hiveConfig: hiveConfig, time, debug: debug, principal: principal)
        {
            this.Do(o: BasicInstruction.Initialize.Create(target: Self, message: HubAgent.Behaviour.Factory.Central(dbConnectionStringGanttPlan, dbConnectionStringMaster, pathToGANTTPLANOptRunner, workTimeGenerator: workTimeGenerator)));
        }

        protected override void Finish()
        {
            // Do not Close agent by Finishmessage from Job
        }

    }
}