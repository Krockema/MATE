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
        private readonly ProductionDomainContext _context;

        public OrderTimelinessViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            // Determine Type and Data
            SimulationType simType = (paramsList[1].Equals("Decentral")) ? SimulationType.Decentral : SimulationType.Central;
            var kpi = _context.Kpis.Where(x => x.KpiType == KpiType.Timeliness
                                    && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                    && x.SimulationType == simType);
            var generateChartTask = Task.Run(() =>
            {

                if (!_context.SimulationWorkschedules.Any())
                {
                    return null;
                }

               Chart chart = new Chart();

                // charttype
                chart.Type = "doughnut";

                // use available hight in Chart
                chart.Options = new PieOptions { MaintainAspectRatio = false , Responsive = true 
                                                    , CutoutPercantage = 80
                                                    , Rotation = 0.8 * Math.PI
                                                    , Circumference = 1.4 * Math.PI
                                                    , Legend = new Legend { Position = "bottom", Display = false }
                                                    , Title = new Title{ Text = "Timeliness", Position = "top", FontSize = 24, FontStyle = "bold"}
                                                };
                var data = new Data
                {
                    Datasets = new List<Dataset>
                        {
                            new PieDataset
                            {
                                BackgroundColor = new[] { "rgba(102, 102, 102, 0.2)", "rgba(75, 192, 192, 0.2)", "rgba(54, 162, 235, 0.2)", "rgba(255, 99, 132, 0.2)","rgba(102, 102, 102, 0.2)" },
                                BorderColor = new[] { "rgba(102, 102, 102, 0.7)", "rgba(75, 192, 192, 0.7)", "rgba(54, 162, 235, 0.7)", "rgba(255, 99, 132, 0.7)", "rgba(102, 102, 102, 0.7)", },
                                BorderWidth = 1,
                           }
                        },
                    Labels = new[] { " ", "Early", "On Time", "Overdue", " " },
                };

                var avg = kpi.Sum(x => x.Value) / kpi.Count();

                var min = kpi.Min(x => x.Value);
                var max = kpi.Max(x => x.Value);
                var end = ((int)Math.Ceiling(max / 100.0)) * 100;


                //data.Datasets[0].Data = new List<double> { 0, (int)(min/end*100), (int)(avg /end*100), (int)(max /end*100), end };
                data.Datasets[0].Data = new List<double> { min, 15, 10, 15, end };

                chart.Data = data;
                return chart;
            });
           
            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            ViewData["percentage"] = "To.Do";
            ViewData["Data"] = kpi.ToList();
            return View($"OrderTimeliness");

        }
    }
}
