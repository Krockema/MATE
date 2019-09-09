using System;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Master40.DB.DataModel
{
    public class T_StockExchange : BaseEntity, IStockExchange, IDemand, IProvider
    {
        public int StockId { get; set; }
        public Guid TrakingGuid { get; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public M_Stock Stock { get; set; }
        public int RequiredOnTime { get; set; }
        public State State { get; set; }
        public decimal Quantity { get; set; }
        // time of withdrawal/insert (wird von der Simulation belegt bei erfuellung des Requests)id
        public int Time { get; set; }
        public ExchangeType ExchangeType { get; set; }
        public StockExchangeType StockExchangeType { get; set; }
        public M_Article GetArticle()
        {
            return Stock.Article;
        }

        public Quantity GetQuantity()
        {
            return new Quantity(Quantity);
        }

        public T_StockExchange()
        {
        }
    }
}
