using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.Simulation.Simulation.SimulationData;

namespace Master40.Simulation.Simulation
{
    public class PurchaseSimulationItem : ISimulationItem
    {
        public PurchaseSimulationItem(ProductionDomainContext context)
        {
            SimulationState = SimulationState.Waiting;
            _context = context;
        }
        private ProductionDomainContext _context;
        public int PurchaseId { get; set; }
        public int PurchasePartId { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public SimulationState SimulationState { get; set; }

        public Task<bool> DoAtStart()
        {
            return null;
        }

        public Task<bool> DoAtEnd<T>(List<TimeTable<T>.MachineStatus> listMachineStatus) where T : ISimulationItem
        {
            var purchasePart = _context.PurchaseParts.Single(a => a.Id == PurchasePartId);
            var stock = _context.Stocks.Single(a => a.ArticleForeignKey == purchasePart.ArticleId);
            stock.Current += purchasePart.Quantity;
            _context.StockExchanges.Add(new StockExchange()
            {
                ExchangeType = ExchangeType.Insert,
                Quantity = purchasePart.Quantity,
                StockId = stock.Id,
                RequiredOnTime = purchasePart.Purchase.DueTime
            });
            purchasePart.State = State.Finished;
            _context.SaveChanges();
            var provider = _context.UpdateStateDemandProviderPurchaseParts();
            _context.UpdateStateDemandRequester(provider);
            return null;
        }
    }
}
