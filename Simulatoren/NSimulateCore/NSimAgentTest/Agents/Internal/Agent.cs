using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSimulate;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents.Internal
{
    public abstract class Agent : Process
    {
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
                    yield return new WaitConditionInstruction(() => InstructionQueue.Count > 0);
                }

                // If there are Instructions
                var doTask = InstructionQueue.Dequeue();
                // Proceed with each one by one - Methods to call MUST be implemented by the Derived Agent itself
                var method = this.GetType().GetMethod(doTask.MethodName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                {
                    throw new NotImplementedException();
                }

                // call Method.
                var invokeReturn = method.Invoke(this, new object[] { doTask }) ;
            }
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
                    targetAgent: this.Creator,
                    sourceAgent: this
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
        /// <param name="sourceAgent"></param>
        public void CreateAndEnqueueInstuction(string methodName, object objectToProcess, Agent targetAgent, Agent sourceAgent)
        {
            // Create And Enqueue
            targetAgent.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = methodName,
                ObjectToProcess = objectToProcess,
                ObjectType = objectToProcess.GetType(),
                SourceAgent = sourceAgent,
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
                Console.WriteLine("Time(" + Context.TimePeriod + ").Agent(" + Name + ") : " + msg);
            }
        }



    }
}