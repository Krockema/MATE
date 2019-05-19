using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Enums;

namespace Zpp
{
    public class StockManager
    {
        private readonly ProductionDomainContext _productionDomainContext;
        private IPurchaseManager _purchaseManager;
        ProductionManager _productionManager;
        private readonly Queue<T_StockExchange> _stockExchanges = new Queue<T_StockExchange>();

        public StockManager(ProductionDomainContext productionDomainContext, ProductionManager productionManager, IPurchaseManager purchaseManager)
        {
            _productionDomainContext = productionDomainContext;
            _productionManager = productionManager;
            _purchaseManager = purchaseManager;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleToOrder"></param>
        /// <param name="dueTime"></param>
        /// <param name="neededQuantity"></param>
        /// <returns>missingQuantity, quantity that was NOT fulfilled by stock</returns>
        public decimal Order(M_Article articleToOrder, int dueTime, decimal neededQuantity)
        {
            M_Stock stock =
                _productionDomainContext.Stocks.Single(x =>
                    x.ArticleForeignKey == articleToOrder.Id);

            decimal missingQuantity = neededQuantity;

            // try to satisfy order by stock
            if (stock.Current > 0)
            {
                missingQuantity = satisfyByStock(stock, dueTime, neededQuantity);
            }
            
            // if LB < MB --> create new stockNeed
            if (stock.Min > stock.Current)
            {
                createStockNeed(stock);
            }
            

            return missingQuantity;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="stock"></param>
        /// <param name="dueTime"></param>
        /// <param name="neededQuantity"></param>
        /// <returns>missingQuantity, quantity that was NOT fulfilled by stock</returns>
        private decimal satisfyByStock(M_Stock stock, int dueTime, decimal neededQuantity)
        {
            if (stock.Current > 0)
            {
                return neededQuantity;
            }
            
            decimal missingQuantity = neededQuantity;

            T_StockExchange stockExchange = new T_StockExchange();
            stockExchange.StockId = stock.Id;
            stockExchange.ExchangeType = ExchangeType.Withdrawal;
            stockExchange.State = State.Created;
            
            missingQuantity = stock.Current - neededQuantity;
            
            // stock need can be completely fulfilled
            if (missingQuantity >= 0)
            {
                stockExchange.Quantity = neededQuantity;
                missingQuantity = 0;
            }
            else
            {
                stockExchange.Quantity = stock.Current;
                missingQuantity = Math.Abs(missingQuantity);
            }

            _stockExchanges.Enqueue(stockExchange);
            
            // update stock
            stock.Current = stock.Current - stockExchange.Quantity;
            _productionDomainContext.Update(stock);

            return missingQuantity;
        }

        private void createStockNeed(M_Stock stock)
        {
            // TODO
        }
    }
}