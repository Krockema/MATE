using System;
using Master40.Agents.Agents.Internal;
using Master40.DB.Models;

namespace Master40.Agents.Agents
{
    public class MachineAgent : Agent
    {

        // Agent to register your Services
        private readonly DirectoryAgent _directoryAgent;
        private ComunicationAgent _comunicationAgent;
        private Machine _machine { get; }

        public enum InstuctionsMethods
        {
            SetComunicationAgent
        }

        public MachineAgent(Agent creator, string name, bool debug, DirectoryAgent directoryAgent, Machine machine) : base(creator, name, debug)
        {
            _directoryAgent = directoryAgent;
            _machine = machine;
            RegisterService();
        }


        /// <summary>
        /// Register the Machine in the System.
        /// </summary>
        public void RegisterService()
        {
            _directoryAgent.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = DirectoryAgent.InstuctionsMethods.GetOrCreateComunicationAgentForType.ToString(),
                ObjectToProcess = this._machine.MachineGroup.Name,
                ObjectType = typeof(string),
                SourceAgent = this
            });
        }


        /// <summary>
        /// Callback
        /// </summary>
        /// <param name="objects"></param>
        private void SetComunicationAgent(InstructionSet objects)
        {
            // Debug Message
            DebugMessage(" got Called by :" + objects.SourceAgent.Name);

            // save Cast to expected object
            _comunicationAgent  = objects.ObjectToProcess as ComunicationAgent;

            // throw if cast failed.
            if (_comunicationAgent == null)
                throw new ArgumentException("Could not Cast Communication Agent from InstructionSet.ObjectToProcess");

            DebugMessage("Successfull Registred Service at : " + _comunicationAgent.Name);
        }
    }
}