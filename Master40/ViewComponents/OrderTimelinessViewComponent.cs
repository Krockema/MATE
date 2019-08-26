using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;
using Master40.DB.Enums;

namespace Master40.ViewComponents
{
    public partial class OrderTimelinessViewComponent : ViewComponent
    {
        private readonly ResultContext _context;

        public OrderTimelinessViewComponent(ResultContext context)
        {
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            // Determine Type and Data
            SimulationType simType = (paramsList[index: 1].Equals(value: "Decentral")) ? SimulationType.Decentral : SimulationType.Central;
            var kpi = _context.Kpis.Where(predicate: x => x.KpiType == KpiType.Timeliness
                                    && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                    && x.SimulationNumber == Convert.ToInt32(paramsList[2])
                                    && x.SimulationType == simType);
            var generateChartTask = Task.Run(function: () =>
            {

                if (!_context.SimulationOperations.Any())
                {
                    return null;
                }

               Chart chart = new Chart();

                // charttype
                chart.Type = Enums.ChartType.Doughnut;

                // use available hight in Chart
                chart.Options = new PieOptions { MaintainAspectRatio = false , Responsive = true 
                                                    , CutoutPercentage =  80
                                                    , Rotation = 0.8 * Math.PI
                                                    , Circumference = 1.4 * Math.PI
                                                    , Legend = new Legend { Position = "bottom", Display = false }
                                                    , Title = new Title{ Text = "Timeliness", Position = "top", FontSize = 24, FontStyle = "bold"}
                                                };

                var cc = new ChartColors();
                var data = new Data
                {
                    Datasets = new List<Dataset>
                        {
                            new PieDataset
                            {
                                BackgroundColor = new[] { cc.Get(4, 0.3),cc.Get(4, 0.3)  },
                                BorderColor = new[] { cc.Get(4, 0.8),cc.Get(4, 0.8)},
                                BorderWidth = 1,
                           }
                        },
                    Labels = new[] { "Early", "Overdue"},
                };

                var avg = kpi.Sum(selector: x => x.Value) / kpi.Count() * 100;

                //var end = ((int)Math.Ceiling(max / 100.0)) * 100;


                //data.Datasets[0].Data = new List<double> { 0, (int)(min/end*100), (int)(avg /end*100), (int)(max /end*100), end };
                data.Datasets[index: 0].Data = new List<double> { avg, 100-avg };
                chart.Data = data;
                return chart;
            });
            // create JS to Render Chart.
            ViewData[index: "chart"] = await generateChartTask;
            ViewData[index: "Type"] = paramsList[index: 1];
            ViewData[index: "Percentage"] = Math.Round(value: kpi.Sum(selector: x => x.Value) / kpi.Count()*100, digits: 0);
            ViewData[index: "Data"] = kpi.ToList();
            return View(viewName: $"OrderTimeliness");

        }
    }
}
