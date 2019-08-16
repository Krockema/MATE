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

        public override bool Action(object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder msg: StartOrder(msg.GetObjectFromMessage); break;
                case BasicInstruction.ProvideArticle msg: TryFinishOrder(msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Startup with Creating Dispo Agent for current Item.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="orderItem"></param>
        public void StartOrder(T_CustomerOrderPart orderItem)
        {
            // create Request Item
            _fArticle = orderItem.ToRequestItem(requester: Agent.Context.Self, Agent.CurrentTime);
            // Tell Guardian to create Dispo Agent
            var agentSetup = AgentSetup.Create(Agent, DispoAgent.Behaviour.Factory.Get(Agent.Behaviour.SimulationType));
            var instruction = Guardian.Instruction.CreateChild.Create(agentSetup, Agent.Guardian);
            Agent.Send(instruction);
            // init end
        }

        /// <summary>
        /// TODO: Test Finish.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="fArticle"></param>
        public void TryFinishOrder(FArticle fArticle)
        {
            Agent.DebugMessage("Dispo Said Done.");
            //var localItem = Agent.Get<FRequestItem>(REQUEST_ITEM);
            _fArticle = fArticle.UpdateFinishedAt(Agent.CurrentTime);

            // try to Finish if time has come
            if (Agent.CurrentTime >= _fArticle.DueTime)
            {
                Agent.Send(Supervisor.Instruction.OrderProvided.Create(_fArticle, Agent.ActorPaths.SystemAgent.Ref));
                Agent.VirtualChildren.Remove(Agent.Sender);
                Agent.TryToFinish();
            }
        }


    }
}
