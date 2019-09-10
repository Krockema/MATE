using System.Linq;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Agents.Guardian;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;

namespace Master40.XUnitTest.Preparations
{
    public class AgentMoc : Agent, IAgent
    {
        IActorRef IAgent.Guardian => _actorPaths.Guardians.Single(predicate: x => x.Key == _guardianType).Value;
        private readonly ActorPaths _actorPaths;
        private readonly GuardianType _guardianType;
        

        public static AgentMoc CreateAgent(ActorPaths actorPaths, IActorRef principal, IBehaviour behaviour, GuardianType guardianType)
        {
            return new AgentMoc(actorPaths: actorPaths,
                                   time: 0,
                                  debug: false,
                              principal: principal,
                              behaviour: behaviour, 
                           guardianType: guardianType);
        }
        public static ActorPaths CreateActorPaths(TestKit testKit, IActorRef simContext)
        {
            var simSystem = testKit.CreateTestProbe();
            var inbox = Inbox.Create(system: testKit.Sys);
            var agentPaths = new ActorPaths(simulationContext: simContext, systemMailBox: inbox.Receiver);
            agentPaths.SetSupervisorAgent(systemAgent: simSystem);
            agentPaths.Guardians.Add(GuardianType.Contract, testKit.CreateTestProbe());
            agentPaths.Guardians.Add(GuardianType.Dispo, testKit.CreateTestProbe());
            agentPaths.Guardians.Add(GuardianType.Production, testKit.CreateTestProbe());
            return agentPaths;
        }

        private AgentMoc(ActorPaths actorPaths, long time, bool debug, IActorRef principal, IBehaviour behaviour, GuardianType guardianType)
            : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            _actorPaths = actorPaths;
            _guardianType = guardianType;
            this.InitializeAgent(behaviour: behaviour);
        }
    }
}