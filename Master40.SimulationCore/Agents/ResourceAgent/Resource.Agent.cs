using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.HubAgent;
using Master40.SimulationCore.Helper;
using System.Collections.Generic;
using Master40.SimulationCore.Types;
using static FAgentInformations;
using static FResourceInformations;
using static FResourceTypes;

namespace Master40.SimulationCore.Agents.ResourceAgent
{
    public partial class Resource : Agent
    {
        // public Constructor
        public static Props Props(ActorPaths actorPaths, M_Resource resource, long time, bool debug, IActorRef principal)
        {
            return Akka.Actor.Props.Create(factory: () => new Resource(actorPaths, resource, time, debug, principal));
        }

        public Resource(ActorPaths actorPaths, M_Resource resource, long time, bool debug, IActorRef principal) : base(actorPaths: actorPaths, time: time, debug: debug, principal: principal)
        {
        }

        protected override void OnInit(IBehaviour o)
        {
            Behaviour.AfterInit();
        }
    }
}