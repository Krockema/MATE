using System;
using NSimAgentTest.Agents.Internal;
namespace NSimAgentTest.Agents
{
    public class MachineAgent : Agent
    {

        // Agent to register your Services
        private readonly DirectoryAgent _directoryAgent;
        private ComunicationAgent _comunicationAgent;
        private string _machineType { get; set; }

        public enum InstuctionsMethods
        {
            SetComunicationAgent
        }

        public MachineAgent(Agent creator, string name, bool debug, DirectoryAgent directoryAgent, string machineType) : base(creator, name, debug)
        {
            _directoryAgent = directoryAgent;
            _machineType = machineType;
            RegisterService();
        }


        /// <summary>
        /// Register the Machine in the System.
        /// </summary>
        public void RegisterService()
        {
            _directoryAgent.InstructionQueue.Enqueue(new InstructionSet
            {
                MethodName = DirectoryAgent.InstuctionsMethods.GetCreateComunicationAgentForType.ToString(),
                ObjectToProcess = this._machineType,
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
            if (DebugThis)
            {
                Console.WriteLine(this.Name + " got Called by :" + objects.SourceAgent.Name);
            }

            // save Cast to expected object
            _comunicationAgent  = objects.ObjectToProcess as ComunicationAgent;

            // throw if cast failed.
            if (_comunicationAgent == null)
            {
                throw new ArgumentException("Could not Cast Communication Agent from InstructionSet.ObjectToProcess");
            }
            Console.WriteLine("Successfull Registred Service at :" + _comunicationAgent.Name);

        }
    }
}