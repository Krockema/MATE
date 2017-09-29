using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using MathNet.Numerics.Statistics;

namespace Master40.Tools.Simulation
{
    public static class CalculateKpis
    {
        /// <summary>
        /// calls all implemented Kpi-calculating methods
        /// </summary>
        /// <param name="context"></param>
        /// <param name="simulationId"></param>
        /// <param name="simulationType"></param>
        /// <param name="simulationNumber"></param>
        public static void CalculateAllKpis(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber)
        {
            CalculateLeadTime(context, simulationId,  simulationType,  simulationNumber);
            CalculateMachineUtilization(context, simulationId,  simulationType,  simulationNumber);
            CalculateTimeliness(context,simulationId,  simulationType,  simulationNumber);
            ArticleStockEvolution(context, simulationId, simulationType, simulationNumber);
            CalculateLayTimes(context, simulationId, simulationType, simulationNumber);
        }

        private static void CalculateLayTimes(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber)
        {
            foreach (var article in context.Articles.Include(a => a.Stock))
            {
                var exchanges = context.StockExchanges.Where(a => a.StockId == article.Stock.Id).ToList();
                var layTimesList = new List<int>();
                while (exchanges.Any())
                {
                    var withdrawal = exchanges.FirstOrDefault(a => a.ExchangeType == ExchangeType.Withdrawal);
                    if (withdrawal == null) break;
                    if (exchanges.IndexOf(withdrawal) == 0)
                    {
                        exchanges.Remove(withdrawal);
                        continue;
                    }
                    var insertion = exchanges.ElementAt(exchanges.IndexOf(withdrawal) - 1);
                    layTimesList.Add(withdrawal.Time - insertion.Time);
                    if (insertion.Quantity > withdrawal.Quantity)
                    {
                        exchanges.Remove(withdrawal);
                        exchanges.ElementAt(exchanges.IndexOf(insertion)).Quantity -= withdrawal.Quantity;
                    }
                    else if (insertion.Quantity == withdrawal.Quantity)
                    {
                        exchanges.Remove(withdrawal);
                        exchanges.Remove(insertion);
                    }
                    else
                    {
                        exchanges.Remove(insertion);
                        exchanges.ElementAt(exchanges.IndexOf(withdrawal)).Quantity -= insertion.Quantity;
                    }
                }
                if (layTimesList.Count > 0)
                {
                    context.Kpis.Add(new Kpi()
                    {
                    IsKpi = true,
                    Value = layTimesList.Average(),
                    Name = article.Name,
                    KpiType = KpiType.LayTime,
                    SimulationConfigurationId = simulationId,
                    SimulationNumber = simulationNumber,
                    ValueMax = layTimesList.Max(),
                    ValueMin = layTimesList.Min(),
                    SimulationType = simulationType,
                    });
                }
            }
            context.SaveChanges();
        }

        /// <summary>
        /// must be called after filling SimulationSchedules!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="simulationId"></param>
        /// <param name="simulationType"></param>
        /// <param name="simulationNumber"></param>
        public static void CalculateLeadTime(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber)
        {
            //calculate lead times for each product
            var leadTimes = new List<Kpi>();
            var finishedProducts = context.SimulationWorkschedules.Where(a => a.ParentId.Equals("[]"));
            foreach (var product in finishedProducts )
            {
                var endTime = product.End;
                var startTime = context.GetEarliestStart(context, product, simulationType);
                leadTimes.Add(new Kpi(){
                    Value = endTime - startTime,
                    Name = product.Article
                });
            }


            var products = leadTimes.Select(x => x.Name).Distinct();
            var leadTimesBoxPlot = new List<Kpi>();
            //calculate Average per article

            foreach (var product in products)
            {
                var relevantItems = leadTimes.Where(x => x.Name.Equals(product)).Select(x => x.Value);
                // BoxPlot: without bounderys
                var stat = Statistics.FiveNumberSummary(relevantItems);
                leadTimesBoxPlot.Add(new Kpi()
                {
                    Name = product,
                    Value = stat[2],
                    ValueMin = stat[0],
                    ValueMax = stat[4],
                    IsKpi = true,
                    KpiType = KpiType.LeadTime,
                    SimulationConfigurationId = simulationId,
                    SimulationType = simulationType,
                    SimulationNumber = simulationNumber
                });

                // Recalculation for Diagram with cut of outliners
                // calculate bounderies using lower mild fence (1,5) -> source: http://www.statisticshowto.com/upper-and-lower-fences/
                var interQuantileRange = stat[3] - stat[1];
                var uperFence = stat[3] + 1.5 * interQuantileRange;
                var lowerFence = stat[1] - 1.5 * interQuantileRange;

                // cut them from the sample
                relevantItems = relevantItems.Where(x => x > lowerFence && x < uperFence);

                // BoxPlot: without bounderys
                stat = Statistics.FiveNumberSummary(relevantItems);

                // Drop to Database
                foreach (var item in stat)
                {
                    leadTimesBoxPlot.Add(new Kpi()
                    {
                        Name = product,
                        Value = item,
                        ValueMin = 0,
                        ValueMax = 0,
                        IsKpi = false,
                        KpiType = KpiType.LeadTime,
                        SimulationConfigurationId = simulationId,
                        SimulationType = simulationType,
                        SimulationNumber = simulationNumber
                    });
                }
            }
            context.Kpis.AddRange(leadTimesBoxPlot);
            context.SaveChanges();
        }

        public static void CalculateMachineUtilization(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber)
        {
            //get machines
            var machines = context.Machines.Select(a => a.Name).ToList();
            //get SimulationTime
            var simulationTime = context.SimulationWorkschedules.Max(a => a.End);
            //get working time

            var content = context.SimulationWorkschedules.Select(x => new { x.Start, x.End, x.Machine });
            var kpis = content.GroupBy(x => x.Machine).Select(g => new Kpi()
                                                                        {
                                                                            Value = (double)(g.Sum(x => x.End) - g.Sum(x => x.Start)) / simulationTime,
                                                                            Name = g.Key,
                                                                            IsKpi = true,
                                                                            KpiType = KpiType.MachineUtilization,
                                                                            SimulationConfigurationId = simulationId,
                                                                            SimulationType = simulationType,
                                                                            SimulationNumber = simulationNumber
                                                                        }).ToList();
            context.Kpis.AddRange(kpis);
            context.SaveChanges();
        }

        public static void CalculateTimeliness(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber)
        {
            var orderTimeliness = context.Orders.Where(a => a.State == State.Finished)
                                         .Select(x => new {x.Name, x.FinishingTime, x.DueTime});
            if (!orderTimeliness.Any()) return;
            var kpis = orderTimeliness.GroupBy(g => g.Name).Select(o => new Kpi()
            {
                Name = o.Key,
                Value = (double)o.Count(x => (x.DueTime-x.FinishingTime) > 0) / o.Count(),
                ValueMin = (double)o.Min(m => m.FinishingTime),
                ValueMax = (double)o.Max(n => n.FinishingTime),
                Count = o.Count(c => c.Name == o.Key),
                IsKpi = true,
                KpiType = KpiType.Timeliness,
                SimulationConfigurationId = simulationId,
                SimulationType = simulationType,
                SimulationNumber = simulationNumber
            }).ToList();
            
            context.Kpis.AddRange(kpis);
            context.SaveChanges();
        }

        public static void ArticleStockEvolution(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber)
        {
            var stockEvoLutions = context.StockExchanges.Include(x => x.Stock).ThenInclude(x => x.Article)
                //.Where(x => x.SimulationType == simulationType && x.SimulationConfigurationId == simulationId && x.SimulationNumber == simulationNumber)
                .Select(x => new { x.ExchangeType, x.RequiredOnTime, x.Stock.Article.Name, x.Stock.Article.Price, x.Stock.StartValue, x.Quantity })
                .OrderBy(x => x.RequiredOnTime);
            var evoMax = stockEvoLutions.Max(x => x.RequiredOnTime);
            var articles = stockEvoLutions.Select(x => new { x.Name , x.StartValue }).Distinct();

            var kpis = new List<Kpi>();


            foreach (var item in articles)
            {
                var value = (double)item.StartValue;
                var lastKpi = new Kpi();
                var articleEvolutions = stockEvoLutions.Where(x => x.Name.Equals(item.Name));
                foreach (var articleEvolution in articleEvolutions)
                {
                    value = (articleEvolution.ExchangeType == ExchangeType.Insert)
                        ? value + (double)articleEvolution.Quantity
                        : value - (double)articleEvolution.Quantity;

                    if ((int)lastKpi.Count == articleEvolution.RequiredOnTime)
                    {
                        lastKpi.Value = value;
                    } else { 
                        lastKpi = new Kpi
                        {
                            Name = articleEvolution.Name,
                            Value = Convert.ToDouble(value),
                            ValueMin = Convert.ToDouble(value * articleEvolution.Price),
                            ValueMax = 0,
                            Count = Convert.ToDouble(articleEvolution.RequiredOnTime),
                            IsKpi = true,
                            KpiType = KpiType.StockEvolution,
                            SimulationConfigurationId = simulationId,
                            SimulationType = simulationType,
                            SimulationNumber = simulationNumber
                        };
                        kpis.Add(lastKpi);
                    }
                }
            }


           // foreach (var stockEvo in stockEvoLutions)
           // {
           //     double lastValue = (double)stockEvo.StartValue;
           //     double value;
           //     if (kpis.Any(x => x.Name.Equals(stockEvo.Name)))
           //     {
           //         var lastKpi = kpis.Last(x => x.Name.Equals(stockEvo.Name));
           //         if (lastKpi != null)
           //             lastValue = lastKpi.Value;
           //     }
           //
           //     var exType = stockEvo.ExchangeType;
           //     value = (exType == ExchangeType.Insert)
           //         ? lastValue + (double)stockEvo.Quantity
           //         : lastValue - (double)stockEvo.Quantity;
           // }
            context.Kpis.AddRange(kpis);
            context.SaveChanges();
        }
    }
}

