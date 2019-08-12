using System;
using System.Collections.Generic;
using System.Text;
using static FArticles;
using Master40.DB.DataModel;
using Master40.SimulationCore.Agents.DispoAgent;
using Master40.SimulationCore.Agents.SupervisorAgent;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.MessageTypes;
using Master40.DB.Enums;

namespace Master40.SimulationCore.Agents.ContractAgent.Behaviour
{
    partial class Default : MessageTypes.Behaviour
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                        : base(null, simulationType) { }

        internal FArticle fArticle { get; set; }

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
            var agentSetup = AgentSetup.Create(agent, DispoAgent.Behaviour.BehaviourFactory.Get(SimulationType.None));
            var instruction = Guardian.Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
            agent.Send(instruction);
            // init
            // create Request Item
            FArticle item = orderItem.ToRequestItem(requester: agent.Context.Self, agent.CurrentTime);
            // Send Request
            fArticle = item;
        }

        /// <summary>
        /// TODO: Test Finish.
        /// </summary>
        /// <param name="instructionSet"></param>
        public void Finish(Contract agent, FArticle item)
        {
            agent.DebugMessage("Dispo Said Done.");
            //var localItem = agent.Get<FRequestItem>(REQUEST_ITEM);
            item = item.UpdateFinishedAt(agent.CurrentTime);
            fArticle = item;

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
