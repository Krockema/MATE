using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Enums;

namespace Master40.ViewComponents
{
    public class StockEvolutionViewComponent : ViewComponent
    {
        private readonly ResultContext _context;

        public StockEvolutionViewComponent(ResultContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            var generateChartTask = Task.Run(function: () =>
            {
            if (!_context.SimulationOperations.Any())
            {
                return null;
            }

            var simConfig = _context.SimulationConfigurations.Single(predicate: x => x.Id == Convert.ToInt32(paramsList[0]));
            var maxX = Convert.ToInt32(value: Math.Floor(d: (decimal)simConfig.SimulationEndTime / 1000) * 1000);

            Chart chart = new Chart();
            
            // charttype
            chart.Type = Enums.ChartType.Scatter;

            // use available hight in Chart
            chart.Options = new LineOptions()
            {
                MaintainAspectRatio = false,
                Responsive = true,
                Scales = new Scales
                {
                    YAxes = new List<Scale> { new CartesianScale { Id = "first-y-axis", Type = "linear", Display = true,
                        ScaleLabel = new ScaleLabel{ LabelString = "Value in €", Display = true, FontSize = 12 },
                        Ticks = new CartesianLinearTick { Min = 0, Display = true } } },
                    XAxes = new List<Scale> { new CartesianScale { Id = "first-x-axis", Type = "linear", Display = true,
                        Ticks = new CartesianLinearTick{ Max = maxX, Min = 0 , Display = true }, 
                        ScaleLabel = new ScaleLabel { LabelString = "Time in min", Display = true, FontSize = 12 } } },
                },
                Legend = new Legend { Position = "bottom", Display = true, FullWidth = true },
                Title = new Title { Text = "Machine Workloads", Position = "top", FontSize = 24, FontStyle = "bold" }
            };


            SimulationType simType = (paramsList[index: 1].Equals(value: "Decentral")) ? SimulationType.Decentral : SimulationType.Central;
            var kpis = _context.Kpis.Where(predicate: x => x.KpiType == KpiType.StockEvolution
                                               && x.Count <= maxX
                                               && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                               && x.SimulationNumber == Convert.ToInt32(paramsList[2])
                                               && x.SimulationType == simType).OrderBy(keySelector: x => x.Name).ToList();

            var articles = kpis.Select(selector: x => x.Name).Distinct();

            var data = new Data { Datasets = new List<Dataset>() };
            foreach (var article in articles)
            {
                // add zero to start
                var articleKpis = new List<LineScatterData> { new LineScatterData { X = "0", Y = "0" } };
                articleKpis.AddRange(collection: kpis.Where(predicate: x => x.Name == article).OrderBy(keySelector: x => x.Count)
                    .Select(selector: x => new LineScatterData { X = x.Count.ToString(), Y = x.ValueMin.ToString() }).ToList());

                    var lds = new LineScatterDataset()
                    {
                        // min Stock
                        Data = articleKpis,
                        BorderWidth = 1,
                        Label = article,
                        ShowLine = true,
                        //SteppedLine = true,
                        LineTension = 0
                        , Hidden = (article.Equals(value: "Dump-Truck") || article.Equals(value: "Race-Truck")) ? false : true
                        ,YAxisID = "first-y-axis"
                    };
                    data.Datasets.Add(item: lds);

                }
                chart.Data = data;
                return chart;
            });

            // create JS to Render Chart.
            ViewData[index: "chart"] = await generateChartTask;
            ViewData[index: "Type"] = paramsList[index: 1];
            return View(viewName: $"StockEvolution");
        }
    }
}
