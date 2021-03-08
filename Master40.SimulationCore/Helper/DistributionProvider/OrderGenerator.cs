﻿using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer.StoredProcedures;
using Master40.DB.DataModel;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using MathNet.Numerics.Distributions;

namespace Master40.SimulationCore.Helper.DistributionProvider
{
    public class OrderGenerator
    {
        private bool _debug { get; set; }
        private List<int> _productIds { get; set; }
        private Random _seededRandom { get; set; }
        private Exponential _exponential { get; set; }
        private DiscreteUniform _prodVariation { get; set; }
        private ContinuousUniform _duetime { get; set; }
        private ProductionDomainContext _productionDomainContext { get; }

        //TODO Seperate generatr from Production Context.
        public OrderGenerator(Configuration simConfig, ProductionDomainContext productionDomainContext, List<int> productIds)
        {
            _seededRandom = new Random(Seed: simConfig.GetOption<Seed>().Value
                //TODO: Do it better                    
                //+ simConfig.GetOption<SimulationNumber>().Value
                                     );
            _exponential = new Exponential(rate: simConfig.GetOption<OrderArrivalRate>().Value
                                         , randomSource: _seededRandom);
            _productIds = productIds;
            _productionDomainContext = productionDomainContext;

            //get equal distribution from 0 to 1
            _prodVariation = new DiscreteUniform(lower: 0, upper: _productIds.Count() - 1, randomSource: _seededRandom);

            //get equal distribution for duetime
            //TODO: Change Option from int to double
            _duetime = new ContinuousUniform(lower: Convert.ToDouble(simConfig.GetOption<MinDeliveryTime>().Value)
                                                    , upper: Convert.ToDouble(simConfig.GetOption<MaxDeliveryTime>().Value)
                                                    , randomSource: _seededRandom);

        }

        public OrderGenerator(Configuration simConfig,  List<int> productIds)
        {
            _seededRandom = new Random(Seed: simConfig.GetOption<Seed>().Value
                //TODO: Do it better                    
                //+ simConfig.GetOption<SimulationNumber>().Value
            );
            _exponential = new Exponential(rate: simConfig.GetOption<OrderArrivalRate>().Value
                , randomSource: _seededRandom);
            _productIds = productIds;

            //get equal distribution from 0 to 1
            _prodVariation = new DiscreteUniform(lower: 0, upper: _productIds.Count() - 1, randomSource: _seededRandom);

            //get equal distribution for duetime
            _duetime = new ContinuousUniform(lower: simConfig.GetOption<MinDeliveryTime>().Value
                , upper: simConfig.GetOption<MaxDeliveryTime>().Value
                , randomSource: _seededRandom);
            
        }


        public T_CustomerOrder GetNewRandomOrder(long time)
        {
            var creationTime = (long)Math.Round(value: _exponential.Sample(), mode: MidpointRounding.AwayFromZero);

            //get which product is to be ordered
            var productId = _productIds.ElementAt(index: _prodVariation.Sample());

            var due = ArticleStatistics.DeliveryDateEstimator(productId, _duetime.Sample(), _productionDomainContext);
            // The old way
            //create order and orderpart, duetime is creationtime + 1 day
            due = time + creationTime + due;
            System.Diagnostics.Debug.WriteLineIf(condition: _debug, message: "Product(" + productId + ")" + ";" + time + ";" + due);

            // only Returns new Order does not save context.
            var order = _productionDomainContext.CreateNewOrder(articleId: productId, amount: 1, creationTime: time + creationTime, dueTime: due);
            return order;
        }
    }
}
