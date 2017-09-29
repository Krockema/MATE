using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Master40.DB.Enums;

namespace Master40.ViewComponents
{
    public class IdlePeriodViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public IdlePeriodViewComponent(ProductionDomainContext context)
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

            Chart chart = new Chart();

            // charttype
            chart.Type = "scatter";

            // use available hight in Chart
            chart.Options = new LineOptions()
            {
                MaintainAspectRatio = false,
                Responsive = true,

                Legend = new Legend { Position = "bottom", Display = true, FullWidth = true },
                Title = new Title { Text = "Stock Evolution", Position = "top", FontSize = 24, FontStyle = "bold" }
            };


            SimulationType simType = (paramsList[1].Equals("Decentral")) ? SimulationType.Decentral : SimulationType.Central;
            var kpis = _context.Kpis.Where(x => x.KpiType == KpiType.StockEvolution
                                               && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                               && x.SimulationType == simType);

            var articles = kpis.Select(x => x.Name).Distinct();

            var data = new Data { Datasets = new List<Dataset>() };
            foreach (var article in articles)
            {
                // add zero to start
                var articleKpis = new List<LineScatterData> { new LineScatterData { x = 0, y = 0 } };
                articleKpis.AddRange(kpis.Where(x => x.Name == article).OrderBy(x => x.Count)
                    .Select(x => new LineScatterData { x = x.Count, y = x.ValueMin }).ToList());

                    var lds = new LineScatterDataset()
                    {
                        // min Stock
                        Data = articleKpis,
                        BorderWidth = 1,
                        Label = article,
                        ShowLine = true,
                        SteppedLine = true,
                        LineTension = 0
                        , Hidden = (article.Equals("Dump-Truck") || article.Equals("Race-Truck")) ? false : true
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
