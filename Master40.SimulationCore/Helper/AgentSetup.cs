using Akka.Actor;
using Master40.SimulationCore.Agents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Helper
{
    public class AgentSetup
    {
        public static AgentSetup Create(Agent agent)
        {
            return new AgentSetup(agent);
        }
        public AgentSetup(Agent agent)
        {
            ActorPaths = agent.ActorPaths;
            Time = agent.CurrentTime;
            Principal = agent.Context.Self;
            Debug = agent.DebugThis;
        }
        public ActorPaths ActorPaths { get; }
        public long Time { get; }
        public IActorRef Principal { get; }
        public bool Debug { get; }
    }
}
