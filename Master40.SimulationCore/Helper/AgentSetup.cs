using Akka.Actor;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Types;

namespace Master40.SimulationCore.Helper
{
    public class AgentSetup
    {
        public static AgentSetup Create(Agent agent, IBehaviour behaviour)
        {
            return new AgentSetup(agent: agent, behaviour: behaviour);
        }
        public AgentSetup(Agent agent, IBehaviour behaviour)
        {
            ActorPaths = agent.ActorPaths;
            Time = agent.CurrentTime;
            Principal = agent.Context.Self;
            Debug = agent.DebugThis;
            Behaviour = behaviour;
        }
        public ActorPaths ActorPaths { get; }
        public long Time { get; }
        public IActorRef Principal { get; }
        public bool Debug { get; }
        public IBehaviour Behaviour { get; }
    }
}
