using System;
using System.Collections.Generic;
using System.Linq;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Environment.Options;
using MathNet.Numerics.Distributions;

namespace Mate.Production.Core.Helper.DistributionProvider
{
    /**
     * Original OrderGenerator works directly on the database, which is way to slow
     */
    public class OrderGeneratorPerformance : IOrderGenerator
    {
        private bool _debug { get; set; }
        private List<int> _productIds { get; set; }
        private Random _seededRandom { get; set; }
        private Exponential _exponential { get; set; }
        private DiscreteUniform _prodVariation { get; set; }
        private DiscreteUniform _duetime { get; set; }

        private readonly List<M_Article> _articles = new List<M_Article>();

        private readonly List<M_BusinessPartner> _businessPartners = new List<M_BusinessPartner>();

        private readonly OrderArrivalRate _orderArrivalRate;

        public OrderGeneratorPerformance(Configuration simConfig, List<int> productIds,
            IEnumerable<M_Article> articles, IEnumerable<M_BusinessPartner> businessPartners)
        {
            _seededRandom = new Random(Seed: simConfig.GetOption<Mate.Production.Core.Environment.Options.Seed>().Value +
                                             simConfig.GetOption<SimulationNumber>().Value);
            _exponential = new Exponential(rate: simConfig.GetOption<OrderArrivalRate>().Value,
                randomSource: _seededRandom);
            _productIds = productIds;

            //get equal distribution from 0 to 1
            _prodVariation = new DiscreteUniform(lower: 0, upper: _productIds.Count() - 1,
                randomSource: _seededRandom);

            //get equal distribution for duetime
            _duetime = new DiscreteUniform(lower: simConfig.GetOption<MinDeliveryTime>().Value,
                upper: simConfig.GetOption<MaxDeliveryTime>().Value, randomSource: _seededRandom);
            
            _articles.AddRange(articles);
            _businessPartners.AddRange(businessPartners);
            _orderArrivalRate = simConfig.GetOption<OrderArrivalRate>();
        }

        public T_CustomerOrder GetNewRandomOrder(long time)
        {
            var creationTime = (long) Math.Round(value: _exponential.Sample(),
                mode: MidpointRounding.AwayFromZero);

            //get which product is to be ordered
            var productId = _productIds.ElementAt(index: _prodVariation.Sample());

            //create order and orderpart, duetime is creationtime + 1 day
            var due = time + creationTime + _duetime.Sample();
            System.Diagnostics.Debug.WriteLineIf(condition: _debug,
                message: "Product(" + productId + ")" + ";" + time + ";" + due);

            // only Returns new Order does not save context.
            var order = CreateNewOrder(articleId: productId, amount: 1,
                creationTime: time + creationTime, dueTime: due);
            return order;
        }

        private T_CustomerOrder CreateNewOrder(int articleId, int amount, long creationTime,
            long dueTime)
        {
            var olist = new List<T_CustomerOrderPart>();
            olist.Add(item: new T_CustomerOrderPart
            {
                Article = _articles.First(predicate: x => x.Id == articleId),
                ArticleId = articleId,
                IsPlanned = false,
                Quantity = amount,
            });

            var bp = _businessPartners.First(predicate: x => x.Debitor);
            var order = new T_CustomerOrder()
            {
                BusinessPartnerId = bp.Id,
                BusinessPartner = bp,
                DueTime = (int) dueTime,
                CreationTime = (int) creationTime,
                Name = _articles.Single(predicate: x => x.Id == articleId).Name,
                CustomerOrderParts = olist
            };
            return order;
        }

        public OrderArrivalRate GetOrderArrivalRate()
        {
            return _orderArrivalRate;
        }
    }
}