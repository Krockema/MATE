using Akka.Actor;
using Akka.Event;
using AkkaSim;
using System.Collections.Generic;
using System.Diagnostics;
using Master40.SimulationImmutables;
using AkkaSim.Interfaces;
using Master40.SimulationCore.Helper;
using System;
using AkkaSim.Definitions;
using Master40.SimulationCore.MessageTypes;

namespace Master40.SimulationCore.Agents
{
    // base Class for Agents
    public abstract class Agent : SimulationElement
    {
        public string Name { get; }
        internal ElementStatus Status { get; private set; }
        internal ActorPaths ActorPaths { get; private set; }
        internal Dictionary<Type, object> Behaviour { get; }
        internal new IUntypedActorContext Context => UntypedActor.Context;
        internal long CurrentTime { get => TimePeriod; }
        internal void TryToFinish() => Finish();
        internal new IActorRef Sender { get => base.Sender; }
        internal Dictionary<string, object> ValueStore { get; }

        // Diagnostic Tools
        private Stopwatch _stopwatch = new Stopwatch();
        public bool DebugThis { get; private set; }
        
        /// <summary>
        /// Basic Agent
        /// </summary>
        /// <param name="actorPaths"></param>
        /// <param name="time">Current time span</param>
        /// <param name="debug">Parameter to activate Debug Messages on Agent level</param>
        public Agent(ActorPaths actorPaths, long time, bool debug) 
            : base(actorPaths.SimulationContext.Ref, time)
        {
            DebugThis = debug;
            Name = Self.Path.Name;
            ActorPaths = actorPaths;
            DebugMessage("I'm alive: " + Self.Path.ToStringWithAddress());
            Behaviour = new Dictionary<Type, object>();
            ValueStore = new Dictionary<string, object>();
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case BasicInstruction.Initialize i: Initialization(i.GetObjectFromMessage); break;
                default: ExecuteMatchingBehave(o); break;
            }
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Get<T>(string name)
        {
            var returns = (T)ValueStore.GetValueOrDefault(name, null);
            if(returns == null)
            {
                if (typeof(T).IsValueType || typeof(T) == typeof(string))
                {
                    returns = default(T);
                }
                else
                {
                    returns = (T)Activator.CreateInstance(typeof(T));
                }
            }
            return returns;
        }

        private void ExecuteMatchingBehave(object o)
        {
            Behaviour.TryGetValue(o.GetType(), out object action);
            if (action is Action<Agent, SimulationMessage>)
            {
                var act = (Action<Agent, ISimulationMessage>)action;
                act(this, o as ISimulationMessage);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(this.Name + " Failed to load matching behaviour for " + o.ToString());
                throw new Exception("Could not find matching behaviour!");
            }
        }

        /// <summary>
        /// Adding Instruction Behaviour releation to the Agent.
        /// </summary>
        /// <param name="behaviourSet"></param>
        private void Initialization(BehaviourSet behaviourSet)
        {
            foreach (var action in behaviourSet.Actions) AddBehave(action.Key, action.Value);
            foreach (var propertie in behaviourSet.Properties) ValueStore.Add(propertie.Key, propertie.Value);
            OnInit(behaviourSet);
        }

        private void AddBehave(Type type, Action<Agent, ISimulationMessage> action)
        {
            this.Behaviour.Add(type, action);
        }

        /// <summary>
        /// Logging the debug Message to Systems.Diagnosics.Debug.WriteLine
        /// </summary>
        /// <param name="msg"></param>
        internal void DebugMessage(string msg)
        {
            if (DebugThis)
            {
                var logItem = "Time(" + TimePeriod + ").Agent(" + Name + ") : " + msg;
                System.Diagnostics.Debug.WriteLine(logItem);
                // TODO: Replace with Logging Agent
                // Statistics.Log.Add(logItem);
            }
        }

        /// <summary>
        /// Creates a Instuction Set and Sends it to the TargetAgent,
        /// ATTENTION !! BE CAERFULL WITH WAITFOR !!
        /// </summary>
        /// <param name="objectToProcess"></param>
        /// <param name="targetAgent"></param>
        /// <param name="waitFor"> Creates a Schedule Object which will pop the Message after the specified time Period!</param>
        public void CreateAndEnqueue(ISimulationMessage instruction, long waitFor = 0)
        {
            if (waitFor == 0)
            {
                _SimulationContext.Tell(message: instruction, sender: Self);
            }
            else
            {
                Schedule(delay: waitFor, message: instruction);
            }
        }

        /// <summary>
        /// Creates a Instuction Set and Sends it to the TargetAgent,
        /// ATTENTION !! BE CAERFULL WITH WAITFOR !!
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="waitFor"></param>
        public void Send(ISimulationMessage instruction, long waitFor = 0)
        {
            if (waitFor == 0)
            {
                _SimulationContext.Tell(message: instruction, sender: Self);
            }
            else
            {
                Schedule(delay: waitFor, message: instruction);
            }
        }

        /// <summary>
        /// Method which is called after Agent Initialisation.
        /// </summary>
        /// <param name="o"></param>
        protected virtual void OnInit(BehaviourSet o) {

        }


        /// <summary>
        /// 
        /// </summary>
        protected override void Finish()
        {
            DebugMessage(Self + " finish has been Called by . " + Sender);
            base.Finish();
        }
    }
}
