using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Nominal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Agents.StorageAgent.Types
{
    public class StockManager
    {
        public StockManager(M_Stock stockElement)
        {
            var initalStock = new T_StockExchange
            {
                StockId = stockElement.Id,
                ExchangeType = ExchangeType.Insert,
                Quantity = stockElement.StartValue,
                State = State.Finished,
                RequiredOnTime = 0,
                Time = 0
            };
            
            Stock = stockElement;
            _providerQueue = new Queue<StockItem>();
            StockExchanges = new List<T_StockExchange> { initalStock };
            if (stockElement.StartValue > 0)
            {
                AddToStock(initalStock);
            }
        }

        /// <summary>
        /// Queue of unused StockExchanges to keep track of Providers
        /// </summary>
        private Queue<StockItem> _providerQueue { get; }
        public M_Stock Stock { get; }
        public decimal? Current => _providerQueue.Sum(x => x.QuantityLeft.GetValue());
        public int Id => Stock.Id;
        public string Name => Stock.Name;
        public double Price => Stock.Article.Price;
        public M_Article Article => Stock.Article;
        /// <summary>
        /// List Of StockExchanges Withdraw as well as Inserts.
        /// </summary>
        public List<T_StockExchange> StockExchanges { get; set; }

        internal T_StockExchange GetStockExchangeByTrackingGuid(Guid exchangeId)
        {
            return StockExchanges.FirstOrDefault(predicate: x => x.TrackingGuid == exchangeId);
        }

        internal void AddToStock(T_StockExchange stockExchange)
        {
            _providerQueue.Enqueue(new StockItem
            {
                QuantityLeft = new Quantity(stockExchange.Quantity)
                , StockExchange = stockExchange
            });
        }

        internal List<T_StockExchange> GetProviderGuidsFor(Quantity totalRequiredQuantity)
        {
            List<T_StockExchange> providerList = new List<T_StockExchange>();
            var requiredQuantity = totalRequiredQuantity;
            while (requiredQuantity.IsGreaterThanZero())
            {
                var provider = _providerQueue.Peek();
                var remainder = provider.QuantityLeft.MinusToZero(requiredQuantity);
                if (remainder)
                {
                    requiredQuantity = provider.QuantityLeft.GetRemainder();
                }
                else
                {
                    requiredQuantity = new Quantity(0);
                }

                if (provider.QuantityLeft.IsZero())
                {
                    _providerQueue.Dequeue();
                }

                providerList.Add(provider.StockExchange);
            }

            return providerList;
        }
    }
}