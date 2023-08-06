using System.Linq;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.StorageAgent.Types;
using Mate.Production.Core.Environment.Records.Central;

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


        public Central(CentralStockDefinitionRecord stockDefinition, SimulationType simType) : base(simulationType: simType)
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

        private void AddOrder(ArticleRecord fArticle)
        {
            _requestedArticles.Add(fArticle);
        }
        private void ProvideOrderAtDue(CentralProvideOrderRecord provideOrder)
        {
            var requestArticle = _requestedArticles.Single(x => x.CustomerOrderId.ToString().Equals(provideOrder.SalesOrderId));

            if (Agent.CurrentTime >= requestArticle.DueTime)
            {   
                Agent.Send(ContractAgent.Contract.Instruction.TryFinishOrder.Create(provideOrder, requestArticle.OriginRequester));
                _requestedArticles.Remove(requestArticle);
                return;
            }

            Agent.Send(Storage.Instruction.Central.ProvideOrderAtDue.Create(provideOrder, Agent.Context.Self), (requestArticle.DueTime - Agent.CurrentTime));

        }


        /// <summary>
        /// Purchase order material initiated by Ganttplan
        /// </summary>
        /// <param name="getObjectFromMessage"></param>
        private void AddPurchase(CentralPurchaseRecord purchase)
        {
            //purchase changes?

            Agent.Send(instruction: Storage.Instruction.Central.PopPurchase.Create(message: purchase, target: Agent.Sender), 
                waitFor: _stockManager.DeliveryPeriod);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getObjectFromMessage"></param>
        private void PopPurchase(CentralPurchaseRecord purchase)
        {
            Agent.DebugMessage($"{purchase.Quantity} {purchase.MaterialId} add to stock ");
            _stockManager.Add(purchase.Quantity);
        }

        private void WithdrawMaterial(StockPostingRecord stockPosting)
        {
            Agent.DebugMessage($"{stockPosting.Quantity} {stockPosting.MaterialId} arrived");
            _stockManager.Remove(stockPosting.Quantity);
            LogValueChange();
        }

        private void InsertMaterial(StockPostingRecord stockPosting)
        {
            Agent.DebugMessage($"{stockPosting.Quantity} {stockPosting.MaterialId} arrived");
            _stockManager.Add(stockPosting.Quantity);
            LogValueChange();
        }
    }
}
