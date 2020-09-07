using Master40.DB.Nominal;
using System;
using Master40.SimulationCore.Agents.StorageAgent.Types;
using static FCentralStockDefinitions;
using static FCentralStockPostings;
using static FCentralPurchases;

namespace Master40.SimulationCore.Agents.StorageAgent.Behaviour
{
    class Central : SimulationCore.Types.Behaviour
    {
        private CentralStockManager _stockManager { get; }
        
        ///RequestStockQuantityAndPurchase

        // PurchaseOrders --> Pop PurchaseEntry --> stock posting at time xxx


        public Central(FCentralStockDefinition stockDefinition, SimulationType simType) : base(simulationType: simType)
        {
            _stockManager = new CentralStockManager(stockDefinition);
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Storage.Instruction.Central.WithdrawMaterial msg:WithdrawMaterial(msg.GetObjectFromMessage); break;
                case Storage.Instruction.Central.InsertMaterial msg: InsertMaterial(msg.GetObjectFromMessage); break;
                case Storage.Instruction.Central.PopPurchase msg: PopPurchase(msg.GetObjectFromMessage); break;
                case Storage.Instruction.Central.AddPurchase msg: AddPurchase(msg.GetObjectFromMessage); break;
                default: 
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Purchase order material initiated by Ganttplan
        /// </summary>
        /// <param name="getObjectFromMessage"></param>
        private void AddPurchase(FCentralPurchase purchase)
        {
            //purchase changes?

            Agent.Send(instruction: Storage.Instruction.Central.PopPurchase.Create(message: purchase, target: Agent.Sender), 
                waitFor: Convert.ToInt32(_stockManager._stockDefinition.DeliveryPeriod));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getObjectFromMessage"></param>
        private void PopPurchase(FCentralPurchase purchase)
        {
            _stockManager.Add(purchase.Quantity);
        }

        private void WithdrawMaterial(FCentralStockPosting stockPosting)
        {
            _stockManager.Remove(stockPosting.Quantity);
        }

        private void InsertMaterial(FCentralStockPosting stockPosting)
        {
            _stockManager.Add(stockPosting.Quantity);
        }
    }
}
