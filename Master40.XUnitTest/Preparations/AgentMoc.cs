﻿using Akka.Actor;
using Akka.TestKit.Xunit;
using AkkaSim.Interfaces;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;

namespace Master40.XUnitTest.Preparations
{
    public class AgentMoc : Agent
    {
        public static AgentMoc CreateAgent(ActorPaths actorPaths, IActorRef principal, IBehaviour behaviour)
        {
            return new AgentMoc(actorPaths: actorPaths,
                                   time: 0,
                                  debug: true,
                              principal: principal,
                              behaviour: behaviour);
        }
        public static ActorPaths CreateActorPaths(TestKit testKit, IActorRef simContext)
        {
            var simSystem = testKit.CreateTestProbe();
            var inbox = Inbox.Create(system: testKit.Sys);
            var agentPaths = new ActorPaths(simulationContext: simContext, systemMailBox: inbox.Receiver);
            agentPaths.SetSupervisorAgent(systemAgent: simSystem);
            return agentPaths;
        }

        private AgentMoc(ActorPaths actorPaths, long time, bool debug, IActorRef principal, IBehaviour behaviour)
            : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
            this.InitializeAgent(behaviour: behaviour);
        }
    }
}