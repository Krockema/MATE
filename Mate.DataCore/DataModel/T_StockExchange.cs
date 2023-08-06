﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Interfaces;
using Mate.DataCore.Nominal;

namespace Mate.DataCore.DataModel
{
    public class T_StockExchange : BaseEntity, IStockExchange, IDemand, IProvider
    {
        public int StockId { get; set; }
        public Guid TrackingGuid { get; set; }
        public int SimulationConfigurationId { get; set; }
        public SimulationType SimulationType { get; set; }
        public int SimulationNumber { get; set; }
        public M_Stock Stock { get; set; }
        public long RequiredOnTime { get; set; }
        public State State { get; set; }
        public decimal Quantity { get; set; }
        // time of withdrawal/insert (wird von der Simulation belegt bei erfuellung des Requests)id
        public long Time { get; set; }
        public ExchangeType ExchangeType { get; set; }

        public StockExchangeType StockExchangeType { get; set; }
        [NotMapped]
        public string ProductionAgent { get; set; }
        [NotMapped]
        public Guid ProductionArticleKey { get; set; }

        public M_Article GetArticle()
        {
            return Stock.Article;
        }

        public Quantity GetQuantity()
        {
            return new Quantity(Quantity);
        }
    }
}
