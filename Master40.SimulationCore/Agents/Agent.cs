using Akka.Actor;
using AkkaSim;
using AkkaSim.Interfaces;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Types;
using NLog;
using System;
using System.Collections.Generic;
using LogLevel = NLog.LogLevel;

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

        // internal IActorRef Guardian { get; }
        internal HashSet<IActorRef> VirtualChildren { get; }
        internal ActorPaths ActorPaths { get; private set; }
        internal IBehaviour Behaviour { get; private set; }
        internal new IUntypedActorContext Context => UntypedActor.Context;
        internal long CurrentTime => TimePeriod;
        internal void TryToFinish() => Finish();
        internal new IActorRef Sender => base.Sender;
        // internal LogWriter LogWriter { get; set; }
        // Diagnostic Tools
        public bool DebugThis { get; private set; }

        /// <summary>
        /// Basic Agent
        /// </summary>
        /// <param name="actorPaths"></param>
        /// <param name="time">Current time span</param>
        /// <param name="debug">Parameter to activate Debug Messages on Agent level</param>
        /// <param name="principal">If not set, put IActorRefs.Nobody</param>
        protected Agent(ActorPaths actorPaths, long time, bool debug, IActorRef principal) 
            : base(simulationContext: actorPaths.SimulationContext.Ref, time: time)
        {
            DebugThis = debug;
            Name = Self.Path.Name;
            ActorPaths = actorPaths;
            VirtualParent = principal;
            VirtualChildren = new HashSet<IActorRef>();
            DebugMessage(msg: "I'm alive: " + Self.Path.ToStringWithAddress());
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case BasicInstruction.Initialize i: InitializeAgent(behaviour: i.GetObjectFromMessage); break;
                case BasicInstruction.ChildRef c: AddChild(childRef: c.GetObjectFromMessage); break;
                case BasicInstruction.Break msg: PostAdvanceBreak(); break;
                default:
                    if (!Behaviour.Action(message: (ISimulationMessage)o))
                        throw new Exception(message: this.Name + " is sorry, he doesn't know what to do!");
                    break;
            }
        }

        private void AddChild(IActorRef childRef)
        {
            DebugMessage(msg: "Try to add child: " + childRef.Path.Name);
            VirtualChildren.Add(item: childRef);
            
            OnChildAdd(childRef: childRef);
        }
        protected virtual void OnChildAdd(IActorRef childRef)
        {
            DebugMessage(msg: this.Name + " Child created.");            
        }

        /// <summary>
        /// Adding Instruction Behaviour relation to the Agent.
        /// Could be simplified, but may required later.
        /// </summary>
        /// <param name="behaviour"></param>
        protected void InitializeAgent(IBehaviour behaviour)
        {
            behaviour.Agent = this;
            this.Behaviour = behaviour;
            DebugMessage(msg: " INITIALIZED ");
            if (VirtualParent != ActorRefs.Nobody)
            {
                DebugMessage(msg: " PARENT INFORMED ");
                Send(instruction: BasicInstruction.ChildRef.Create(message: Self, target: VirtualParent));
            }
            
            OnInit(o: behaviour);
        }
        
        /// <summary>
        /// Logging the debug Message to Systems.Diagnosics.Debug.WriteLine
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="LogLevel" optional="true">Nlog.LogLevel</para>
        internal void DebugMessage(string msg, LogLevel logLevel = null)
        {
            //if (!DebugThis) return;
            if(logLevel == null) 
                logLevel = LogLevel.Debug;
            //Debug.WriteLine(message: logItem, category: "AgentMessage");
            Logger.Log(logLevel
                        , "Time({TimePeriod}).Agent({Name}): {msg}"
                        , new object[] { TimePeriod, Name, msg });
        }

        /// <summary>
        /// Logging the debug Message to Systems.Diagnosics.Debug.WriteLine
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="LogLevel" optional="true">Nlog.LogLevel</para>
        internal void DebugMessage(string msg, string customLogger, LogLevel logLevel = null)
        {
            Logger CustomLogger = LogManager.GetLogger(customLogger);
            //if (!DebugThis) return;
            if (logLevel == null)
                logLevel = LogLevel.Debug;
            //Debug.WriteLine(message: logItem, category: "AgentMessage");
            CustomLogger.Log(logLevel
                        , "Time({TimePeriod}).Agent({Name}): {msg}"
                        , new object[] { TimePeriod, Name, msg });
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
            DebugMessage(msg: Self + " finish has been called by " + Sender);
            base.Finish();
        }
        // protected override void PostAdvance()
        // {
        //     // 
        // }

        private void PostAdvanceBreak()
        {
            // if (Agent.CurrentTime == 2000)
            // {
            //     System.Diagnostics.Debugger.Break();
            // }
        }

    }
}
