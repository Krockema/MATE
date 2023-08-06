using System;
using System.Linq;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.StorageAgent.Types;
using Mate.Production.Core.Helper;
using static FArticles;
using static FCentralProvideOrders;
using static FCentralPurchases;
using static FCentralStockDefinitions;
using static FCentralStockPostings;

namespace Mate.Production.Core.Agents.StorageAgent.Behaviour
{
    class Central : Core.Types.Behaviour
    {
        private CentralStockManager _stockManager { get; }

        internal ArticleList _requestedArticles { get; set; } = new ArticleList();

        internal void LogValueChange()
                        => ((Storage)Agent).LogValueChange(_stockManager.MaterialName
                            , _stockManager.MaterialType
                            , _stockManager.Value);


        public Central(FCentralStockDefinition stockDefinition, SimulationType simType) : base(simulationType: simType)
        {
            _stockManager = new CentralStockManager(stockDefinition);
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Storage.Instruction.Central.AddOrder msg: AddOrder(msg.GetObjectFromMessage); break;
                case Storage.Instruction.Central.ProvideOrderAtDue msg: ProvideOrderAtDue(msg.GetObjectFromMessage); break;
                case Storage.Instruction.Central.WithdrawMaterial msg: WithdrawMaterial(msg.GetObjectFromMessage); break;
                case Storage.Instruction.Central.InsertMaterial msg: InsertMaterial(msg.GetObjectFromMessage); break;
                case Storage.Instruction.Central.PopPurchase msg: PopPurchase(msg.GetObjectFromMessage); break;
                case Storage.Instruction.Central.AddPurchase msg: AddPurchase(msg.GetObjectFromMessage); break;
                default: 
                    return false;
            }
            return true;
        }

        private void AddOrder(FArticle fArticle)
        {
            _requestedArticles.Add(fArticle);
        }
        private void ProvideOrderAtDue(FCentralProvideOrder provideOrder)
        {
            var requestArticle = _requestedArticles.Single(x => x.CustomerOrderId.ToString().Equals(provideOrder.SalesOrderId));

            if (Agent.CurrentTime >= requestArticle.DueTime)
            {   
                Agent.Send(ContractAgent.Contract.Instruction.TryFinishOrder.Create(provideOrder, requestArticle.OriginRequester));
                _requestedArticles.Remove(requestArticle);
                return;
            }

            Agent.Send(Storage.Instruction.Central.ProvideOrderAtDue.Create(provideOrder, Agent.Context.Self), (requestArticle.DueTime - Agent.CurrentTime).ToTimeSpan());

        }


        /// <summary>
        /// Purchase order material initiated by Ganttplan
        /// </summary>
        /// <param name="getObjectFromMessage"></param>
        private void AddPurchase(FCentralPurchase purchase)
        {
            //purchase changes?

            Agent.Send(instruction: Storage.Instruction.Central.PopPurchase.Create(message: purchase, target: Agent.Sender), 
                waitFor: _stockManager.DeliveryPeriod.ToTimeSpan());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getObjectFromMessage"></param>
        private void PopPurchase(FCentralPurchase purchase)
        {
            Agent.DebugMessage($"{purchase.Quantity} {purchase.MaterialId} add to stock ");
            _stockManager.Add(purchase.Quantity);
        }

        private void WithdrawMaterial(FCentralStockPosting stockPosting)
        {
            Agent.DebugMessage($"{stockPosting.Quantity} {stockPosting.MaterialId} arrived");
            _stockManager.Remove(stockPosting.Quantity);
            LogValueChange();
        }

        private void InsertMaterial(FCentralStockPosting stockPosting)
        {
            Agent.DebugMessage($"{stockPosting.Quantity} {stockPosting.MaterialId} arrived");
            _stockManager.Add(stockPosting.Quantity);
            LogValueChange();
        }
    }
}
