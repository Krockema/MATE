using Akka.Actor;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;

namespace Master40.SimulationCore.MessageTypes
{
    public abstract class Behaviour: IBehaviour
    {
        public Behaviour(Func<IUntypedActorContext, AgentSetup, IActorRef> childMaker = null
                          , Dictionary<string, object> properties = null
                          , object obj = null)
        {
            ChildMaker = childMaker;
            Properties = (properties == null) ? new Dictionary<string, object>() : properties;
            Object = obj;
        }
        public virtual bool Action(Agent agent, object message) { throw new Exception("No Action is implemented!"); }
        //public Action<Agent, ISimulationMessage> Action { get; }
        public IReadOnlyDictionary<string, object> Properties { get; }
        public object Object { get; }
        public Func<IUntypedActorContext, AgentSetup, IActorRef> ChildMaker { get; }
    }
}
