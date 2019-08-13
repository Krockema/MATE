using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.SupervisorAgent;
using Master40.SimulationCore.Helper;
using static FArticles;

namespace Master40.SimulationCore.Agents.ContractAgent.Behaviour
{
    public class Default : SimulationCore.Types.Behaviour, IDefaultProperties
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                        : base(null, simulationType) { }

        public FArticle fArticle { get; internal set; }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder m: StartOrder(agent, m.GetObjectFromMessage); break;
                case Contract.Instruction.Finish msg: Finish(agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Startup with Creating Dispo Agent for current Item.
        /// </summary>
        /// <param agent="ContractAgent"></param>
        /// <param startOrder="ISimulationMessage"></param>
        public void StartOrder(Agent agent, T_CustomerOrderPart orderItem)
        {

            // Tell Guadian to create Dispo Agent
            var agentSetup = AgentSetup.Create(agent, DispoAgent.Behaviour.Factory.Get(SimulationType.None));
            var instruction = Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
            agent.Send(instruction);
            // init
            // create Request Item
            fArticle = orderItem.ToRequestItem(requester: agent.Context.Self, agent.CurrentTime);
        }

        /// <summary>
        /// TODO: Test Finish.
        /// </summary>
        /// <param name="instructionSet"></param>
        public void Finish(Agent agent, FArticle item)
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
                agent.TryToFinish();
            }
        }


    }
}
