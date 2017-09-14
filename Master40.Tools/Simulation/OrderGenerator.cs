using System;
using System.Collections.Generic;
using Master40.DB.Data.Context;
using System.Linq;
using Master40.DB.Interfaces;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.Tools.Simulation
{
    public static class OrderGenerator
    {
        public static void GenerateOrders(ProductionDomainContext context, int simulationId)
        {
            var time = 0;
            //get products by searching for articles without parents
            var productIds = context.ArticleBoms.Where(b => b.ArticleParentId == null).Select(a => a.ArticleChildId).ToList();
            for (var i = 0; i < context.SimulationConfigurations.Single(a => a.Id == simulationId).OrderQuantity; i++)
            {
                //get equal distribution from 0 to 1
                var randomProductNumber = MathNet.Numerics.Distributions.DiscreteUniform.Sample(0,productIds.Count()-1);
                //define the time between each new order
                time += 50 + (int)Math.Round(MathNet.Numerics.Distributions.Exponential.Sample(0.25),MidpointRounding.AwayFromZero);
                //get which product is to be ordered
                var productId = productIds.ElementAt(randomProductNumber);
                //create order and orderpart, duetime is creationtime + 1 day
                context.CreateNewOrder(productId, 1, time, time + 1440);
            }
        }

        public static List<double> TestUniformDistribution(int amount)
        {
            var samples = new List<double>();
            for (var i = 0; i < amount; i++)
            {
               samples.Add(MathNet.Numerics.Distributions.DiscreteUniform.Sample(0, 1000)/1000.00);
            }
            return samples;
        }

        public static List<double> TestExponentialDistribution(int amount)
        {
            var samples = new List<double>();
            for (var i = 0; i < amount; i++)
            {
                samples.Add(MathNet.Numerics.Distributions.Exponential.Sample(0.25));
            }
            return samples;
        }
    }
}
