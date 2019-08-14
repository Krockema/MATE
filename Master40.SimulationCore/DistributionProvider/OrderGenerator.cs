using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.DistributionProvider
{
    public class OrderGenerator
    {
        private bool _debug { get; set; }
        private List<int> _productIds { get; set; }
        private Random _seededRandom { get; set; }
        private Exponential _exponential { get; set; }
        private DiscreteUniform _prodVariation { get; set; }
        private DiscreteUniform _duetime { get; set; }
        private ProductionDomainContext _productionDomainContext { get; }

        public OrderGenerator(Configuration simConfig, ProductionDomainContext productionDomainContext, List<int> productIds)
        {
            _seededRandom = new Random(simConfig.GetOption<Seed>().Value
                                     + simConfig.GetOption<SimulationNumber>().Value);
            _exponential = new Exponential(rate: simConfig.GetOption<OrderArrivalRate>().Value
                                         , randomSource: _seededRandom);
            _productIds = productIds;
            _productionDomainContext = productionDomainContext;

            //get equal distribution from 0 to 1
            _prodVariation = new DiscreteUniform(0, _productIds.Count() - 1, _seededRandom);

            //get equal distribution for duetime
            _duetime = new DiscreteUniform(simConfig.GetOption<MinDeliveryTime>().Value
                                                    , simConfig.GetOption<MaxDeliveryTime>().Value
                                                    , _seededRandom);
        }



        public T_CustomerOrder GetNewRandomOrder(long time)
        {
            var creationTime = (long)Math.Round(_exponential.Sample(), MidpointRounding.AwayFromZero);

            //get which product is to be ordered
            var productId = _productIds.ElementAt(_prodVariation.Sample());

            //create order and orderpart, duetime is creationtime + 1 day
            var due = time + creationTime + _duetime.Sample();
            System.Diagnostics.Debug.WriteLineIf(_debug, "Product(" + productId + ")" + ";" + time + ";" + due);

            // only Returns new Order does not save context.
            var order = _productionDomainContext.CreateNewOrder(productId, 1, time + creationTime, due);
            return order;
        }
    }
}
