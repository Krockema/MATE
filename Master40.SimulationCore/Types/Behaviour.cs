using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;

namespace Master40.SimulationCore.Types
{
    public abstract class Behaviour: IBehaviour
    {
        protected Behaviour(Func<IUntypedActorContext, AgentSetup, IActorRef> childMaker = null
                          , object obj = null
                          , SimulationType simulationType = SimulationType.None)
        {
            ChildMaker = childMaker;
            Object = obj;
            SimulationType = simulationType;
        }
        public virtual bool Action(object message) { throw new Exception(message: "No Action is implemented!"); }
        //public Action<Agent, ISimulationMessage> Action { get; }
        public object Object { get; }
        public SimulationType SimulationType { get; }
        public Func<IUntypedActorContext, AgentSetup, IActorRef> ChildMaker { get; }
        public Agent Agent { get; set; }
        public virtual bool AfterInit() { return true; }
    }
}
