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
            var simConfig = context.SimulationConfigurations.Single(a => a.Id == simulationId);
            var final = simConfig.Time >= simConfig.SimulationEndTime;
            CalculateLeadTime(context, simulationId,  simulationType,  simulationNumber, final);
            CalculateMachineUtilization(context, simulationId,  simulationType,  simulationNumber, final);
            CalculateTimeliness(context,simulationId,  simulationType,  simulationNumber, final);
            ArticleStockEvolution(context, simulationId, simulationType, simulationNumber, final);
            CalculateLayTimes(context, simulationId, simulationType, simulationNumber, final);
        }

        public static void CalculateLayTimes(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber, bool final)
        {
            var simConfig = context.SimulationConfigurations.Single(a => a.Id == simulationId);
            foreach (var article in context.Articles.Include(a => a.Stock))
            {
                List<StockExchange> exchanges;
                if (final)
                    exchanges = context.StockExchanges.Where(a => a.StockId == article.Stock.Id).ToList();
                else
                {
                    exchanges = context.StockExchanges.Where(a =>
                        a.StockId == article.Stock.Id && a.Time >= simConfig.Time - simConfig.DynamicKpiTimeSpan).ToList();
                }
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
                        IsKpi = final,
                        Value = layTimesList.Average(),
                        Name = article.Name,
                        KpiType = KpiType.LayTime,
                        SimulationConfigurationId = simulationId,
                        SimulationNumber = simulationNumber,
                        ValueMax = layTimesList.Max(),
                        ValueMin = layTimesList.Min(),
                        SimulationType = simulationType,
                        Time = simConfig.Time
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
        public static void CalculateLeadTime(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber, bool final)
        {
            var simConfig = context.SimulationConfigurations.Single(a => a.Id == simulationId);
            //calculate lead times for each product
            var finishedProducts = final ? context.SimulationWorkschedules.Where(a => a.ParentId.Equals("[]") && a.SimulationConfigurationId == simulationId).ToList()
                                         : context.SimulationWorkschedules.Where(a => a.ParentId.Equals("[]") && a.SimulationConfigurationId == simulationId && a.Start >= simConfig.Time - simConfig.DynamicKpiTimeSpan && a.End <= simConfig.Time).ToList();
            var leadTimes = (from product in finishedProducts
                let endTime = product.End
                let startTime = context.GetEarliestStart(context, product, simulationType)
                select new Kpi()
                {
                    Value = endTime - startTime,
                    Name = product.Article
                }).ToList();


            var products = leadTimes.Select(x => x.Name).Distinct();
            var leadTimesBoxPlot = new List<Kpi>();
            //calculate Average per article

            foreach (var product in products)
            {
                var relevantItems = leadTimes.Where(x => x.Name.Equals(product)).Select(x => x.Value);
                // BoxPlot: without bounderys
                var stat = relevantItems.FiveNumberSummary();
                leadTimesBoxPlot.Add(new Kpi()
                {
                    Name = product,
                    Value = stat[2],
                    ValueMin = stat[0],
                    ValueMax = stat[4],
                    IsKpi = final,
                    KpiType = KpiType.LeadTime,
                    SimulationConfigurationId = simulationId,
                    SimulationType = simulationType,
                    SimulationNumber = simulationNumber,
                    Time = simConfig.Time
                });

                // Recalculation for Diagram with cut of outliners
                // calculate bounderies using lower mild fence (1,5) -> source: http://www.statisticshowto.com/upper-and-lower-fences/
                var interQuantileRange = stat[3] - stat[1];
                var uperFence = stat[3] + 1.5 * interQuantileRange;
                var lowerFence = stat[1] - 1.5 * interQuantileRange;

                // cut them from the sample
                relevantItems = relevantItems.Where(x => x > lowerFence && x < uperFence);

                // BoxPlot: without bounderys
                stat = relevantItems.FiveNumberSummary();

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

        public static void CalculateMachineUtilization(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber, bool final)
        {
            var simConfig = context.SimulationConfigurations.Single(a => a.Id == simulationId);
            //get machines
            var machines = context.Machines.Select(a => a.Name).ToList();
            //get SimulationTime
            var simulationTime = context.SimulationWorkschedules.Max(a => a.End);
            //get working time

            var content = final
                ? context.SimulationWorkschedules.Select(x => new {x.Start, x.End, x.Machine}).ToList()
                : context.SimulationWorkschedules.Where(a => a.Start >= simConfig.Time - simConfig.DynamicKpiTimeSpan)
                    .Select(x => new {x.Start, x.End, x.Machine}).ToList();
            var kpis = content.GroupBy(x => x.Machine).Select(g => new Kpi()
                                                                        {
                                                                            Value = (double)(g.Sum(x => x.End) - g.Sum(x => x.Start)) / simulationTime,
                                                                            Name = g.Key,
                                                                            IsKpi = final,
                                                                            KpiType = KpiType.MachineUtilization,
                                                                            SimulationConfigurationId = simulationId,
                                                                            SimulationType = simulationType,
                                                                            SimulationNumber = simulationNumber,
                                                                            Time = simConfig.Time
                                                                        }).ToList();
            context.Kpis.AddRange(kpis);
            context.SaveChanges();
        }

        public static void CalculateTimeliness(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber, bool final)
        {
            var simConfig = context.SimulationConfigurations.Single(a => a.Id == simulationId);
            var orderTimeliness = final ? context.Orders.Where(a => a.State == State.Finished)
                                            .Select(x => new {x.Name, x.FinishingTime, x.DueTime}).ToList()
                                        : context.Orders.Where(a => a.State == State.Finished && a.FinishingTime >= simConfig.Time - simConfig.DynamicKpiTimeSpan)
                                            .Select(x => new { x.Name, x.FinishingTime, x.DueTime }).ToList();
            if (!orderTimeliness.Any()) return;
            var kpis = orderTimeliness.GroupBy(g => g.Name).Select(o => new Kpi()
            {
                Name = o.Key,
                Value = (double)o.Count(x => (x.DueTime-x.FinishingTime) > 0) / o.Count(),
                ValueMin = (double)o.Min(m => m.FinishingTime),
                ValueMax = (double)o.Max(n => n.FinishingTime),
                Count = o.Count(c => c.Name == o.Key),
                IsKpi = final,
                KpiType = KpiType.Timeliness,
                SimulationConfigurationId = simulationId,
                SimulationType = simulationType,
                SimulationNumber = simulationNumber,
                Time = simConfig.Time
            }).ToList();
            
            context.Kpis.AddRange(kpis);
            context.SaveChanges();
        }

        public static void ArticleStockEvolution(ProductionDomainContext context, int simulationId, SimulationType simulationType, int simulationNumber, bool final)
        {
            var simConfig = context.SimulationConfigurations.Single(a => a.Id == simulationId);
            var stockEvoLutionsContext = context.StockExchanges.Include(x => x.Stock).ThenInclude(x => x.Article)
                .Where(x => x.SimulationType == simulationType && x.SimulationConfigurationId == simulationId && x.SimulationNumber == simulationNumber);
            if (final)
                stockEvoLutionsContext = stockEvoLutionsContext.Where(a => a.Time >= simConfig.Time - simConfig.DynamicKpiTimeSpan);
            var stockEvoLutions = stockEvoLutionsContext.Select(x => new { x.ExchangeType, x.RequiredOnTime, x.Stock.Article.Name, x.Stock.Article.Price, x.Stock.StartValue, x.Quantity })

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
                            IsKpi = final,
                            KpiType = KpiType.StockEvolution,
                            SimulationConfigurationId = simulationId,
                            SimulationType = simulationType,
                            SimulationNumber = simulationNumber,
                            Time = simConfig.Time
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

