using Akka.Actor;
using Akka.TestKit.Xunit;
using AkkaSim.Interfaces;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;

namespace Master40.XUnitTest.Moc
{
    public class AgentMoc : Agent
    {
        public static AgentMoc Create(ActorPaths actorPaths, IActorRef principal)
        {
            return new AgentMoc(actorPaths: actorPaths,
                                   time: 0,
                                  debug: true,
                              principal: principal);
        }
        public static ActorPaths Create(TestKit testKit, IActorRef simContext)
        {
            var simSystem = testKit.CreateTestProbe();
            var inbox = Inbox.Create(testKit.Sys);
            var agentPaths = new ActorPaths(simContext, inbox.Receiver);
            agentPaths.SetSupervisorAgent(simSystem);
            return agentPaths;
        }

        private AgentMoc(ActorPaths actorPaths, long time, bool debug, IActorRef principal)
            : base(actorPaths, time, debug, principal)
        {

        }
    }
}