using Master40.DB.Models;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System.Collections.Generic;

namespace Master40.SimulationCore.Agents
{
    public class ContractBehaviour : Behaviour
    {
        private ContractBehaviour(Dictionary<string, object> properties) : base(null, properties) { }

        /// <summary>
        /// Returns the Default Behaviour Set for Contract Agent.
        /// </summary>
        /// <returns></returns>
        public static ContractBehaviour Get()
        {
            var properties = new Dictionary<string, object>();

            return new ContractBehaviour(properties);
        }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder m: StartOrder((Contract)agent, m.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Startup with Creating Dispo Agent for current Item.
        /// </summary>
        /// <param agent="ContractAgent"></param>
        /// <param startOrder="ISimulationMessage"></param>
        public void StartOrder(Contract agent, OrderPart orderItem)
        {

            // Tell Guadian to create Dispo Agent
            var agentSetup = AgentSetup.Create(agent);
            var instruction = Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
            agent.Send(instruction);
            // init
            // create Request Item
            RequestItem requestItem = orderItem.ToRequestItem(requester: agent.Context.Self);
            // Send Request
            agent.ValueStore.Add(Contract.Properties.REQUEST_ITEM, requestItem);
        }

        /// <summary>
        /// TODO: Test Finish.
        /// </summary>
        /// <param name="instructionSet"></param>
        public void Finish(Contract agent, RequestItem item)
        {
            agent.DebugMessage("Dispo Said Done.");
            var localItem = agent.Get<RequestItem>(Contract.Properties.REQUEST_ITEM);
            // try to Finish if time has come
            if (agent.CurrentTime >= item.DueTime)
            {
                agent.Send(Supervisor.Instruction.OrderProvided.Create(item, agent.ActorPaths.SystemAgent.Ref));
                agent.TryFinialize();
            }
        }
    }
}
