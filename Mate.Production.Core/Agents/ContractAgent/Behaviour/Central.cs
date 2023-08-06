using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.SupervisorAgent;
using Mate.Production.Core.Environment.Records.Central;
using Mate.Production.Core.Helper;
using System;

namespace Mate.Production.Core.Agents.ContractAgent.Behaviour
{
    public class Central : Types.Behaviour, IDefaultProperties
    {
        internal Central(SimulationType simulationType = SimulationType.None)
                        : base(childMaker: null, simulationType: simulationType) { }

        public ArticleRecord _fArticle { get; internal set; }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Contract.Instruction.StartOrder msg: StartOrder(orderItem: msg.GetObjectFromMessage); break;
                case Contract.Instruction.TryFinishOrder msg: TryFinishOrder(msg.GetObjectFromMessage); break;
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
                                            , remainingDuration: TimeSpan.Zero
                                            , currentTime: Agent.CurrentTime);


            Agent.DebugMessage(msg: $"Start Order");
            Agent.Send(DirectoryAgent.Directory.Instruction.Central.ForwardAddOrder.Create(_fArticle,
                Agent.ActorPaths.StorageDirectory.Ref));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="fArticle"></param>
        public void TryFinishOrder(CentralProvideOrderRecord order)
        {
            Agent.DebugMessage(msg: "Ready to Deliver");
            //var localItem = Agent.Get<FRequestItem>(REQUEST_ITEM);

            // try to Finish if time has come
            if (Agent.CurrentTime >= _fArticle.DueTime)
            {
                _fArticle = _fArticle.SetProvided()
                                    .UpdateFinishedAt(Agent.CurrentTime)
                                    .UpdateProvidedAt(order.MaterialFinishedAt);
                Agent.DebugMessage(msg: $"Article delivered in time {_fArticle.DueTime == Agent.CurrentTime} {order.MaterialName} {order.MaterialId} due: {_fArticle.DueTime} current: {Agent.CurrentTime}!");
                Agent.Send(instruction: DirectoryAgent.Directory.Instruction.Central.ForwardWithdrawMaterial.Create(new StockPostingRecord(order.MaterialId,1), target: Agent.ActorPaths.StorageDirectory.Ref));
                Agent.Send(instruction: Supervisor.Instruction.OrderProvided.Create(message: _fArticle, target: Agent.ActorPaths.SystemAgent.Ref));
                Agent.VirtualChildren.Remove(item: Agent.Sender);
                Agent.TryToFinish();
            }
        }

        public void EstimateForwardEnd(DateTime estimatedEnd)
        {
            Agent.DebugMessage(
                msg:
                $"Scheduling finished with earliest End at: {estimatedEnd}");

        }


    }
}
