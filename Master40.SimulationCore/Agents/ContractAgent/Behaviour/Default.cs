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

        public FArticle _fArticle { get; internal set; }

        public override bool Action(Agent agent, object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder msg: StartOrder(agent, msg.GetObjectFromMessage); break;
                case BasicInstruction.ProvideArticle msg: TryFinishOrder(agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Startup with Creating Dispo Agent for current Item.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="orderItem"></param>
        public void StartOrder(Agent agent, T_CustomerOrderPart orderItem)
        {
            // create Request Item
            _fArticle = orderItem.ToRequestItem(requester: agent.Context.Self, agent.CurrentTime);
            // Tell Guardian to create Dispo Agent
            var agentSetup = AgentSetup.Create(agent, DispoAgent.Behaviour.Factory.Get(agent.Behaviour.SimulationType));
            var instruction = Guardian.Instruction.CreateChild.Create(agentSetup, agent.Guardian);
            agent.Send(instruction);
            // init end
        }

        /// <summary>
        /// TODO: Test Finish.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="fArticle"></param>
        public void TryFinishOrder(Agent agent, FArticle fArticle)
        {
            agent.DebugMessage("Dispo Said Done.");
            //var localItem = agent.Get<FRequestItem>(REQUEST_ITEM);
            _fArticle = fArticle.UpdateFinishedAt(agent.CurrentTime);

            // try to Finish if time has come
            if (agent.CurrentTime >= _fArticle.DueTime)
            {
                agent.Send(Supervisor.Instruction.OrderProvided.Create(_fArticle, agent.ActorPaths.SystemAgent.Ref));
                agent.VirtualChilds.Remove(agent.Sender);
                agent.TryToFinish();
            }
        }


    }
}
