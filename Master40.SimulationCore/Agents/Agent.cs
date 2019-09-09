using Akka.Actor;
using AkkaSim;
using AkkaSim.Interfaces;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

namespace Master40.SimulationCore.Agents
{
    // base Class for Agents
    public abstract class Agent : SimulationElement
    {
        public string Name { get; }
        /// <summary>
        /// VirtualParrent is his Principal Agent
        /// </summary>
        internal IActorRef VirtualParent { get; }
        /// <summary>
        /// Holds the last Known Status for each Child Entity
        /// </summary>
        internal Dictionary<IActorRef, ElementStatus> VirtualChilds { get; }
        internal ElementStatus Status { get; set; }
        internal ActorPaths ActorPaths { get; private set; }
        internal IBehaviour Behaviour { get; private set; }
        internal new IUntypedActorContext Context => UntypedActor.Context;
        internal long CurrentTime { get => TimePeriod; }
        internal void TryToFinish() => Finish();
        internal new IActorRef Sender { get => base.Sender; }
        private dynamic ValueStore { get; }
            = new ExpandoObject();
        // Diagnostic Tools
        private Stopwatch _stopwatch = new Stopwatch();
        public bool DebugThis { get; private set; }
        
        /// <summary>
        /// Basic Agent
        /// </summary>
        /// <param name="actorPaths"></param>
        /// <param name="time">Current time span</param>
        /// <param name="debug">Parameter to activate Debug Messages on Agent level</param>
        /// <param name="principal">If not set, put IActorRefs.Nobody</param>
        public Agent(ActorPaths actorPaths, long time, bool debug, IActorRef principal) 
            : base(actorPaths.SimulationContext.Ref, time)
        {
            DebugThis = debug;
            Name = Self.Path.Name;
            ActorPaths = actorPaths;
            VirtualParent = principal;
            VirtualChilds = new Dictionary<IActorRef, ElementStatus>();
            DebugMessage("I'm alive: " + Self.Path.ToStringWithAddress());
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case BasicInstruction.Initialize i: InitializeAgent(i.GetObjectFromMessage); break;
                case BasicInstruction.ChildRef c: AddChild(c.GetObjectFromMessage); break;
                default:
                    if (!Behaviour.Action(this, (ISimulationMessage)o))
                        throw new Exception(this.Name + " is sorry, he doesnt know what to do!");
                    break;
            }
        }

        /// <summary>
        /// Returns the requested Property, if null an empty new property is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Get<T>(string propertyName)
        {
            var expandoDict = ValueStore as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))            
                return (T)expandoDict[propertyName];

            throw new Exception("Propertie not Found!");
        }
        public void Set(string propertyName, object obj)
        {
            var expandoDict = ValueStore as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = obj;
            else
                expandoDict.Add(propertyName, obj);
        }

        private void AddChild(IActorRef childRef)
        {
            DebugMessage("Try to add child: " + childRef.Path.Name);
            VirtualChilds.Add(childRef, ElementStatus.Created);
            
            OnChildAdd(childRef);
        }
        protected virtual void OnChildAdd(IActorRef childRef)
        {
            DebugMessage(this.Name + "Child Created.");            
        }

        /// <summary>
        /// Adding Instruction Behaviour releation to the Agent.
        /// Could be simplified, but may required later.
        /// </summary>
        /// <param name="behaviourSet"></param>
        private void InitializeAgent(IBehaviour behaviour)
        {
            this.Behaviour = behaviour;
            foreach (var propertie in behaviour.Properties) ((IDictionary<string, object>)ValueStore).Add(propertie.Key, propertie.Value);
            DebugMessage(" INITIALIZED ");
            if (VirtualParent != ActorRefs.Nobody)
            {
                DebugMessage(" PARRENT INFORMED ");
                Send(BasicInstruction.ChildRef.Create(Self, VirtualParent));
            }
            
            OnInit(behaviour);
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
                Debug.WriteLine(logItem, "AgentMessage");
                // TODO: Replace with Logging Agent
                // Statistics.Log.Add(logItem);
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
        protected virtual void OnInit(IBehaviour o) {

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
