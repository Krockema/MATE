using System.Collections.Generic;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.SupervisorAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;

namespace Master40.SimulationCore.Agents.ContractAgent
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
                case Contract.Instruction.Finish msg: Finish((Contract)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Startup with Creating Dispo Agent for current Item.
        /// </summary>
        /// <param agent="ContractAgent"></param>
        /// <param startOrder="ISimulationMessage"></param>
        public void StartOrder(Contract agent, T_CustomerOrderPart orderItem)
        {

            // Tell Guadian to create Dispo Agent
            var agentSetup = AgentSetup.Create(agent, DispoAgent.Behaviour.BehaviourFactory.Get(DB.Enums.SimulationType.None));
            var instruction = Guardian.Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
            agent.Send(instruction);
            // init
            // create Request Item
            FRequestItem requestItem = orderItem.ToRequestItem(requester: agent.Context.Self);
            // Send Request
            agent.Set(Contract.Properties.REQUEST_ITEM, requestItem);
        }

        /// <summary>
        /// TODO: Test Finish.
        /// </summary>
        /// <param name="instructionSet"></param>
        public void Finish(Contract agent, FRequestItem item)
        {
            agent.DebugMessage("Dispo Said Done.");
            //var localItem = agent.Get<FRequestItem>(REQUEST_ITEM);
            item = item.UpdateFinishedAt(agent.CurrentTime);
            agent.Set(Contract.Properties.REQUEST_ITEM, item);
            
            // try to Finish if time has come
            if (agent.CurrentTime >= item.DueTime)
            {
                agent.Send(Supervisor.Instruction.OrderProvided.Create(item, agent.ActorPaths.SystemAgent.Ref));
                agent.VirtualChilds.Remove(agent.Sender);
                agent.TryFinialize();
            }
        }
    }
}
