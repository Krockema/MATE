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
        private readonly ProductionDomainContext _context;

        public StockEvolutionViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            var generateChartTask = Task.Run(() =>
            {
            if (!_context.SimulationWorkschedules.Any())
            {
                return null;
            }

            var simConfig = _context.SimulationConfigurations.Single(x => x.Id == Convert.ToInt32(paramsList[0]));
            var maxX = Convert.ToInt32(Math.Floor((decimal)simConfig.SimulationEndTime / 1000) * 1000);

            Chart chart = new Chart();
            
            // charttype
            chart.Type = "scatter";

            // use available hight in Chart
            chart.Options = new LineOptions()
            {
                MaintainAspectRatio = false,
                Responsive = true,
                Scales = new Scales
                {
                    YAxes = new List<Scale> { new CartesianScale { Id = "first-y-axis", Type = "linear", Display = true, ScaleLabel = new ScaleLabel{ LabelString = "Value in €", Display = true, FontSize = 12 } } },
                    XAxes = new List<Scale> { new CartesianScale { Id = "first-x-axis", Type = "linear", Display = true,
                        Ticks = new CartesianLinearTick{ Max = maxX, Min = 0 , Display = true }, 
                        ScaleLabel = new ScaleLabel { LabelString = "Time in min", Display = true, FontSize = 12 } } },
                },
                Legend = new Legend { Position = "bottom", Display = true, FullWidth = true },
                Title = new Title { Text = "Machine Workloads", Position = "top", FontSize = 24, FontStyle = "bold" }
            };


            SimulationType simType = (paramsList[1].Equals("Decentral")) ? SimulationType.Decentral : SimulationType.Central;
            var kpis = _context.Kpis.Where(x => x.KpiType == KpiType.StockEvolution
                                               && x.Count <= maxX
                                               && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                               && x.SimulationNumber == Convert.ToInt32(paramsList[2])
                                               && x.SimulationType == simType).OrderBy(x => x.Name).ToList();

            var articles = kpis.Select(x => x.Name).Distinct();

            var data = new Data { Datasets = new List<Dataset>() };
            foreach (var article in articles)
            {
                // add zero to start
                var articleKpis = new List<LineScatterData> { new LineScatterData { x = "0", y = "0" } };
                articleKpis.AddRange(kpis.Where(x => x.Name == article).OrderBy(x => x.Count)
                    .Select(x => new LineScatterData { x = x.Count.ToString(), y = x.ValueMin.ToString() }).ToList());

                    var lds = new LineScatterDataset()
                    {
                        // min Stock
                        Data = articleKpis,
                        BorderWidth = 1,
                        Label = article,
                        ShowLine = true,
                        //SteppedLine = true,
                        LineTension = 0
                        , Hidden = (article.Equals("Dump-Truck") || article.Equals("Race-Truck")) ? false : true
                        ,YAxisID = "first-y-axis"
                    };
                    data.Datasets.Add(lds);

                }
                chart.Data = data;
                return chart;
            });

            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            return View($"StockEvolution");
        }
    }
}
