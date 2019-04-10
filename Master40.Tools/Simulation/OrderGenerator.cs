using System;
using System.Collections.Generic;
using Master40.DB.Data.Context;
using System.Linq;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;
using System.Threading.Tasks;
using Master40.DB.DataModel;

namespace Master40.Tools.Simulation
{
    public static class OrderGenerator
    {
        public static Task GenerateOrders(ProductionDomainContext context, SimulationConfiguration simConfig, int simulationNumber)
        {
            return Task.Run(() =>
            {
                GenerateOrdersSyncron(context, simConfig, simulationNumber, false);
            });
        }

        public static bool GenerateOrdersSyncron(ProductionDomainContext context, SimulationConfiguration simConfig, int simulationNumber, bool debug)
        {
            var time = 0;
            var samples = simConfig.OrderQuantity;
            var seed = new Random(simConfig.Seed + simulationNumber);
            var productIds = context.ArticleBoms
                                    .Where(b => b.ArticleParentId == null)
                                    .Select(a => a.ArticleChildId)
                                    .ToList();

            var dist = new Exponential(rate: simConfig.OrderRate, randomSource: seed);
            //get equal distribution from 0 to 1
            var norml = new DiscreteUniform(0, productIds.Count() - 1, seed);
            //get equal distribution for duetime
            var normalDuetime = new DiscreteUniform(1160, 1600, seed); //2160,3600

            double[] exponential = new double[samples]; //new Exponential(0.25, seed);
            int[] prodVariation = new int[samples];
            int[] duetime = new int[samples];
            dist.Samples(exponential);
            norml.Samples(prodVariation);
            normalDuetime.Samples(duetime);
            //get products by searching for articles without parents
            for (int i = 0; i < samples; i++)
            {
                //define the time between each new order
                time += (int)Math.Round(exponential[i], MidpointRounding.AwayFromZero);
                //get which product is to be ordered
                var productId = productIds.ElementAt(prodVariation[i]);

                //create order and orderpart, duetime is creationtime + 1 day

                System.Diagnostics.Debug.WriteLineIf(debug, "Product(" + productId + ")" + ";" + time + ";" + (time + duetime[i]));
                context.Orders.Add(context.CreateNewOrder(productId, 1, time, time + duetime[i]));
            }
            context.SaveChanges();
            return true;
        } 



        public static List<double> TestUniformDistribution(int amount)
        {
            /*var samples = new List<double>();
            for (var i = 0; i < amount; i++)
            {
               samples.Add(MathNet.Numerics.Distributions.DiscreteUniform.Sample(0, 1000)/1000.00);
            }
            return samples;*/
            var samples = new List<double>();
            var dist = new LogNormal(0,0.125);
            for (int i = 0; i < amount; i++)
            {
                var sample = dist.Sample();
                var round = Math.Round(sample*5, MidpointRounding.AwayFromZero);
                if (sample < 5 || sample > 5)
                {
                    var a = 1;
                }
                samples.Add( (int)round);
            }
            return samples;
        }

        public static List<double> TestExponentialDistribution(int amount)
        {
            var seed = 1337;
            var samples = 1000;
            SystemRandomSource randomSource = new SystemRandomSource(seed);
            var dist = new Exponential(rate: 0.25, randomSource: new Random(seed));

            double[] list = new double[1000];
            dist.Samples(list);
            for(var i=0;i<list.Count();i++)
            {
                list[i] *= 10;
            }
            return list.ToList();
        }
    }
}
