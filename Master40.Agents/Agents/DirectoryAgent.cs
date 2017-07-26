using System.Linq;
using Master40.Agents.Agents.Internal;

namespace Master40.Agents.Agents
{
    public class DirectoryAgent : Agent
    {
        public DirectoryAgent(Agent creator, string name, bool debug) : base(creator, name, debug)
        {
            //Instructions.Add(new Instruction {Method = "GetOrCreateComunicationAgentForType", ExpectedObjecType = typeof(string) });       
        }
        public enum InstuctionsMethods
        {
            GetOrCreateComunicationAgentForType,
        }
        

        private void GetOrCreateComunicationAgentForType(InstructionSet objects)
        {
            // debug
            DebugMessage(" got Called by -> " + objects.SourceAgent.Name);

            // find the related Comunication Agent
            var comunicationAgent = ChildAgents.OfType<ComunicationAgent>().ToList()
                                                .FirstOrDefault(x => x.ContractType == objects.ObjectToProcess.ToString());

            // if no Comunication Agent is found, Create one
            if (comunicationAgent == null)
            {
                // Create ComunicationAgent if not existent
                comunicationAgent = new ComunicationAgent(creator: this,
                                                             name: "Comunication ->" + objects.ObjectToProcess, 
                                                            debug: this.DebugThis, 
                                                     contractType: objects.ObjectToProcess.ToString());
                // add Agent Reference.
                ChildAgents.Add(comunicationAgent);
            }

            // Tell the Machine the corrosponding Comunication Agent.
            CreateAndEnqueueInstuction(methodName: MachineAgent.InstuctionsMethods.SetComunicationAgent.ToString(),
                                  objectToProcess: comunicationAgent,
                                      targetAgent: objects.SourceAgent);
        }
    }
}