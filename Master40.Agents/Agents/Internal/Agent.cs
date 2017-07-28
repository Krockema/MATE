using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NSimulate.Instruction;
using Process = NSimulate.Process;

namespace Master40.Agents.Agents.Internal
{
    public abstract class Agent : Process
    {
        // Agent Statistics
        public static List<String> AgentCounter = new List<string>();
        public static int InstructionCounter = 0;
        public static List<AgentStatistic> AgentStatistics = new List<AgentStatistic>();
        private Stopwatch _stopwatch = new Stopwatch();

        // Agent Properties.
        public Guid AgentId { get; }
        internal Agent Creator { get; set; }
        internal List<Agent> ChildAgents { get; set; }
        public string Name { get; set; }
        public bool DebugThis { get; set; }
        internal Status Status { get; set; }
        public Queue<InstructionSet> InstructionQueue { get; set; }

        protected Agent(Agent creator, string name, bool debug)
        {
            AgentId = Guid.NewGuid();
            AgentCounter.Add(this.GetType().Name);
            this.Name = name;
            this.DebugThis = debug;
            this.InstructionQueue = new Queue<InstructionSet>();
            this.ChildAgents = new List<Agent>();
            this.Status = Status.Created;

            // Cheack for Creator Agent
            var creatorsName = "Simulation Context";
            if (creator == null) { Creator = this; }
            else {  this.Creator = creator; creatorsName = Creator.Name; }

            DebugMessage(" created by " + creatorsName + ", GUID: " + AgentId);
        }

        public override IEnumerator<InstructionBase> Simulate()
        {
            // while Simulation is Running
            while (true)
            {
                // Wait for Instructions
                if (InstructionQueue.Count == 0)
                {
                    TimerStop();
                    yield return new WaitConditionInstruction(() => InstructionQueue.Count > 0);
                }
                // Statistic
                InstructionCounter++;
                TimerStart();
                // If there are Instructions
                var doTask = InstructionQueue.Dequeue();
                // Proceed with each one by one - Methods to call MUST be implemented by the Derived Agent itself
                var method = this.GetType().GetMethod(doTask.MethodName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                {
                    throw new NotImplementedException(Name  + "| Source:" + doTask.SourceAgent + "| Method Name: " + doTask.MethodName);
                }

                // call Method.
                var invokeReturn = method.Invoke(this, new object[] { doTask }) ;
            }
        }

        private void TimerStop()
        {
            _stopwatch.Stop();
            AgentStatistics.Add(new AgentStatistic {Agent = this.GetType().Name, ProcessingTime = _stopwatch.ElapsedMilliseconds});
        }

        private void TimerStart()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Finalize the current Agent and Call the Parrent.
        /// </summary>
        internal void Finish()
        {
            if (DebugThis)
            {
                DebugMessage(" Finished Work.");
            }
            // Set State Finish
            this.Status = Status.Finished;
            // Tell Parent
            CreateAndEnqueueInstuction(
                     methodName: "Finished",
                objectToProcess: this,
                    targetAgent: this.Creator
            );
        }


        /// <summary>
        /// check Childs and Call Finish if all in State Finished.
        /// </summary>
        internal void Finished(InstructionSet objects)
        {
            // any Not Finished do noting
            if (ChildAgents.Any(x => x.Status != Status.Finished))
                return;

            // if All finished Clear Resource
            ChildAgents.Clear();
            Status = Status.Ready;

            // if this agent is also Finished tell the Parrent.
            if (Status == Status.Finished)
                Finish();
        }

        /// <summary>
        /// Creates a Instuction Set and Enqueue it to the TargetAgent,
        /// It pushes the Agent to Context.Queue
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="objectToProcess"></param>
        /// <param name="targetAgent"></param>
        public void CreateAndEnqueueInstuction(string methodName, object objectToProcess, Agent targetAgent)
        {
            // Create And Enqueue
            targetAgent.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = methodName,
                ObjectToProcess = objectToProcess,
                ObjectType = objectToProcess.GetType(),
                SourceAgent = this,
            });

            // Re-Activate Process in Context Queue if nesessary
            if (!Context.ProcessesRemainingThisTimePeriod.Contains(targetAgent))
                Context.ProcessesRemainingThisTimePeriod.Enqueue(targetAgent);
        }

        /// <summary>
        /// Impementation of debug msg broker.
        /// </summary>
        /// <param name="msg"></param>
        internal void DebugMessage(string msg)
        {
            if (DebugThis)
            {
                Debug.WriteLine("Time(" + Context.TimePeriod + ").Agent(" + Name + ") : " + msg);
            }
        }



    }
}