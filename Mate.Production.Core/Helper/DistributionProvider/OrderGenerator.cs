using Mate.DataCore.Data.Context;
using Mate.DataCore.Data.Initializer.StoredProcedures;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment;
using Mate.Production.Core.Environment.Options;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mate.Production.Core.Helper.DistributionProvider
{
    public class OrderGenerator
    {
        private bool _debug { get; set; }
        private List<int> _productIds { get; set; }
        private Random _seededRandom { get; set; }
        private Exponential _exponential { get; set; }
        private DiscreteUniform _prodVariation { get; set; }
        private DiscreteUniform _amountVariation { get; set; }
        private ContinuousUniform _duetime { get; set; }
        private MateProductionDb _productionDomainContext { get; }

        //TODO Seperate generatr from Production Context.
        public OrderGenerator(Configuration simConfig, MateProductionDb productionDomainContext, List<int> productIds)
        {
            _debug = true;
            _seededRandom = new Random(Seed: simConfig.GetOption<Environment.Options.Seed>().Value
                //TODO: Do it better                    
                //+ simConfig.GetOption<SimulationNumber>().Value
                                     );
            _exponential = new Exponential(rate: simConfig.GetOption<OrderArrivalRate>().Value
                                         , randomSource: _seededRandom);
            _productIds = productIds;
            _productionDomainContext = productionDomainContext;

            //get equal distribution from 0 to 1
            _prodVariation = new DiscreteUniform(lower: 0, upper: _productIds.Count() - 1, randomSource: _seededRandom);

            //get equal distribution from 0 to 1
            _amountVariation = new DiscreteUniform(lower: simConfig.GetOption<MinQuantity>().Value
                                                    , upper: simConfig.GetOption<MaxQuantity>().Value
                                                    , randomSource: _seededRandom);

            //get equal distribution for duetime
            //TODO: Change Option from int to double
            _duetime = new ContinuousUniform(lower: Convert.ToDouble(simConfig.GetOption<MinDeliveryTime>().Value)
                                                    , upper: Convert.ToDouble(simConfig.GetOption<MaxDeliveryTime>().Value)
                                                    , randomSource: _seededRandom);

        }

        public OrderGenerator(Configuration simConfig,  List<int> productIds)
        {
            _seededRandom = new Random(Seed: simConfig.GetOption<Environment.Options.Seed>().Value
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


        public T_CustomerOrder GetNewRandomOrder(DateTime time)
        {
            var creationTime = TimeSpan.FromMinutes((long)Math.Round(value: _exponential.Sample(), mode: MidpointRounding.AwayFromZero));

            //get which product is to be ordered
            var productId = _productIds.ElementAt(index: _prodVariation.Sample());

            var due = ArticleStatistics.DeliveryDateEstimator(productId, _duetime.Sample(), _productionDomainContext);
            // The old way
            //create order and orderpart, duetime is creationtime + 1 day
            var dueDate = time + creationTime + due;
            System.Diagnostics.Debug.WriteLineIf(condition: _debug, message: "Product(" + productId + ")" + ";" + time + ";" + dueDate);
            
            var amount = _amountVariation.Sample();

            // only Returns new Order does not save context.
            var order = _productionDomainContext.CreateNewOrder(articleId: productId, amount: amount, creationTime: time + creationTime, dueTime: dueDate);
            return order;
        }
    }
}
