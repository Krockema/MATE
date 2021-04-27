using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.DispoAgent;
using Mate.Production.Core.Agents.SupervisorAgent;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Agents.ContractAgent.Behaviour
{
    public class Default : Types.Behaviour, IDefaultProperties
    {
        internal Default(SimulationType simulationType = SimulationType.None)
                        : base(childMaker: null, simulationType: simulationType) { }

        public FArticles.FArticle _fArticle { get; internal set; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder msg: StartOrder(orderItem: msg.GetObjectFromMessage); break;
                case BasicInstruction.ProvideArticle msg: TryFinishOrder(fArticleProvider: msg.GetObjectFromMessage); break;
                case BasicInstruction.JobForwardEnd msg: EstimateForwardEnd(estimatedEnd: msg.GetObjectFromMessage); break;
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
            _fArticle = orderItem.ToRequestItem(requester: Agent.Context.Self
                                            , customerDue: orderItem.CustomerOrder.DueTime
                                            , remainingDuration: 0
                                            , currentTime: Agent.CurrentTime);
            // Tell Guardian to create Dispo Agent
            var agentSetup = AgentSetup.Create(agent: Agent, behaviour: DispoAgent.Behaviour.Factory.Get(simType: Agent.Behaviour.SimulationType));
            var instruction = Guardian.Instruction.CreateChild.Create(setup: agentSetup, target: ((IAgent)Agent).Guardian, source: Agent.Context.Self);
            Agent.Send(instruction: instruction);
            // init end
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="fArticle"></param>
        public void TryFinishOrder(FArticleProviders.FArticleProvider fArticleProvider)
        {
            Agent.DebugMessage(msg: "Ready to Deliver");
            //var localItem = Agent.Get<FRequestItem>(REQUEST_ITEM);

            // try to Finish if time has come
            if (Agent.CurrentTime >= _fArticle.DueTime)
            {
                _fArticle = _fArticle.SetProvided
                                    .UpdateFinishedAt(Agent.CurrentTime)
                                    .UpdateProvidedAt(fArticleProvider.ArticleFinishedAt)
                                    .UpdateStockExchangeId(fArticleProvider.StockExchangeId);
                Agent.DebugMessage(msg: $"Article delivered in time {_fArticle.DueTime == Agent.CurrentTime} {fArticleProvider.ArticleName} {fArticleProvider.ArticleKey} due: {_fArticle.DueTime} current: {Agent.CurrentTime}!");
                Agent.Send(instruction: Dispo.Instruction.WithdrawArticleFromStock.Create(message: fArticleProvider.ArticleKey, target: Agent.Sender));
                Agent.Send(instruction: Supervisor.Instruction.OrderProvided.Create(message: _fArticle, target: Agent.ActorPaths.SystemAgent.Ref));
                Agent.VirtualChildren.Remove(item: Agent.Sender);
                Agent.TryToFinish();
            }
        }

        public void EstimateForwardEnd(long estimatedEnd)
        {
            Agent.DebugMessage(
                msg:
                $"Scheduling finished with earliest End at: {estimatedEnd}");

        }
        public override void OnChildAdd(IActorRef childRef)
        {
            Agent.Send(instruction: Dispo.Instruction.RequestArticle.Create(message: _fArticle, target: childRef));
            Agent.DebugMessage(msg: "Dispo<" + _fArticle.Article.Name + "(OrderId: " + _fArticle.CustomerOrderId + ")>");
        }

    }
}
