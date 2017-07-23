using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NSimAgentTest.Enums;
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
        Status Status { get; set; }
        public Queue<InstructionSet> InstructionQueue { get; set; }
        
        
        protected Agent(Agent creator, string name, bool debug)
        {
            AgentId = Guid.NewGuid();
            this.Name = name;
            this.DebugThis = true;
            this.InstructionQueue = new Queue<InstructionSet>();
            this.ChildAgents = new List<Agent>();
            this.Status = Status.Created;
            
            // Cheack for Creator Agent
            var creatorsName = "System";
            if (creator == null) { Creator = this; }
            else {  this.Creator = creator; creatorsName = Creator.Name; }


            if (DebugThis)
            {
                Console.WriteLine("Agent " + name + " created by " + creatorsName + ", GUID: " + AgentId.ToString());
            }

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


        public void Finish()
        {
            // Set State Finish
            this.Status = Status.Finished;
            // Tell Parent
            Creator.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = "Finished",
                ObjectToProcess = this,
                ObjectType = this.GetType(),
                SourceAgent = this,
            });
        }
    }
}