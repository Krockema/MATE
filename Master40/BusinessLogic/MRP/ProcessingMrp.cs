using Master40.Data;
using Master40.Models;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessingMrp
    {
        List<LogMessage> Logger { get; set; }
        Task Process(int orderId);
    }

    public class ProcessingMrp : IProcessingMrp
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public ProcessingMrp(MasterDBContext context)
        {
            _context = context;
            Logger = new List<LogMessage>();
        }

        public async Task Process(int orderId)
        {

            await Task.Run(async () =>
            {
                // #1 Create Main Demand
                var orderParts = await CreateDemandForOrderPart(orderId);
                // #2 
                foreach (var item in orderParts)
                {
                    // await DoRecustivePlanningAsync(item.Article, item.ArticleId);
                }
            });
        }


        private async Task<List<OrderPart>> CreateDemandForOrderPart(int OrderId)
        {
            var orderParts = await _context.OrderParts.Include(a => a.Article).Where(x => x.OrderId == OrderId).ToListAsync();
            foreach (var item in orderParts)
            {
                var demand = new DemandOrderPart
                {
                    ArticleId = item.ArticleId,
                    OrderPartId = item.OrderPartId,
                    Quantity = item.Quantity,
                };
                _context.Demands.Add(demand);
            };
            _context.SaveChanges();

            //var productionOrder = new DemandForecast(_context).NetRequirement(demand, parent, orderPartId);

            Logger.Add(new LogMessage { Message = "Created Demands", MessageNumber = 1, MessageType = MessageType.info });
            return orderParts;
        }

        private async Task<Article> DoRecustivePlanningAsync(Article article, IDemandToProvider demandRequester, int ArticleId)
        {
            article.ArticleChilds = _context.ArticleBoms.Include(a => a.ArticleChild).Where(a => a.ArticleParentId == ArticleId);
            foreach (var item in article.ArticleChilds)
            {
                var toProduce = GetArticlesStockReservations(article, demandRequester, article.ArticleChilds.First().Quantity * item.Quantity);
                if (toProduce != 0)
                {
                    // Create ProductionOrder
                    // // Create ProductionOrderBom
                    // /// //
               


                }



                // await DoRecustivePlanningAsync(item.ArticleParent, item.ArticleChildId);
            }
            await Task.Yield();
            return article;
        }

        private decimal GetArticlesStockReservations(Article article, IDemandToProvider demandRequester, decimal quantity)
        {
            var stock = _context.Stocks.Include(a => a.DemandStocks)
                           .Single(a => a.ArticleForeignKey == article.ArticleId);

            //TODO: get Reserverd by ~ _context.DemandToProvider.Where(x => x.articleId && x.DemandRequesterId != 0) // summe über Quantity = Reserved Bilden
            // Kann man im DB Model als Static hinterlegt das hier nur noch // _Context.DemandToProvider.GetReservationsForArticle(int ArticleId) gerufen werden muss.
            decimal amountReserved = 0;
            foreach (var demandStock in stock.DemandStocks)
            {
                if (demandStock.StockId == stock.StockId)
                    amountReserved = demandStock.Quantity;
            }

            //plannedStock is the amount of this article in stock after taking out the amount needed

            var required = quantity - stock.Current - amountReserved;
            decimal toProduce = 0;
            decimal toReservation = 0;
            if (required >= 0)
            {
                toReservation = quantity;
            } else
            {
                toReservation = stock.Current - amountReserved;
                toProduce = quantity - toReservation;
            }
            CreateDemandProvider(stock, demandRequester.DemandId, quantity);

            //if there is at least one or more of this article in stock
            if (stock.Current > 0)
            {
                var msg = "Articles in stock: " + article.Name + " " + stock.Current;
                Logger.Add(new LogMessage() { MessageType = MessageType.success, Message = msg });
            }
            return toProduce;
        }

        private void CreateDemandProvider(Stock stock, int demandRequesterId, decimal quantity)
        {
            var demand = new DemandProviderStock
            {
                ArticleId = stock.ArticleForeignKey,
                DemandRequesterId = demandRequesterId,
                Quantity = quantity
            };
            _context.Demands.Add(demand);
            _context.SaveChanges();

            //var productionOrder = new DemandForecast(_context).NetRequirement(demand, parent, orderPartId);

            Logger.Add(new LogMessage { Message = "Created DemandProvider Stock for :" + stock.Name, MessageNumber = 1, MessageType = MessageType.info });
        }

        private void CreateDemand(int ProductionOrderId)
        {
                
        }
    }
}