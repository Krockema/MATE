using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NSimAgentTest.Agents.Internal;
using NSimAgentTest.Enums;
using NSimulate;
using NSimulate.Instruction;

namespace NSimAgentTest.Agents
{
    public class DirectoryAgent : Agent
    {
        public DirectoryAgent(Agent creator, string name, bool debug) : base(creator, name , debug) { }
        public enum InstuctionsMethods
        {
            GetOrCreateComunicationAgentForType,
            GetCreateComunicationAgentForType
        }
        

        private void GetCreateComunicationAgentForType(InstructionSet objects)
        {
            if (DebugThis)
            {
                Console.WriteLine(this.Name + " got Called by :" + objects.SourceAgent.Name);
            }

            // find the related Comunication Agent
            var comunicationAgent = ChildAgents.OfType<ComunicationAgent>().ToList()
                                                .FirstOrDefault(x => x.ContractType == objects.ObjectToProcess.ToString());


            if (comunicationAgent == null)
            {
                // Create ComunicationAgent if not existent
                comunicationAgent = new ComunicationAgent(creator: this,
                                                            name : "Comunication Agent for " + objects.ObjectToProcess, 
                                                            debug: this.DebugThis, 
                                                     contractType: objects.ObjectToProcess.ToString());
                // add Agent Reference.
                ChildAgents.Add(comunicationAgent);
            }
            
            // Tell the Machine the corrosponding Comunication Agent.
            objects.SourceAgent.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = MachineAgent.InstuctionsMethods.SetComunicationAgent.ToString(),
                ObjectToProcess = comunicationAgent,
                ObjectType = typeof(ComunicationAgent),
                SourceAgent = this
            });
        }
    }
}