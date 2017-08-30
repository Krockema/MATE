using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public partial class ProductLeadTimeViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public ProductLeadTimeViewComponent(ProductionDomainContext context)
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
                chart.Type = "doughnut";

                // use available hight in Chart
                chart.Options = new PieOptions
                {
                    MaintainAspectRatio = false,
                    Responsive = true,
                    CutoutPercantage = 80,
                    Rotation = 0.8 * Math.PI,
                    Circumference = 1.4 * Math.PI,
                    Legend = new Legend { Position = "bottom", Display = false },
                    Title = new Title { Text = "Product Lead Time", Position = "top", FontSize = 24, FontStyle = "bold" }
                };
                var data = new Data
                {
                    Datasets = new List<Dataset>
                        {
                            new PieDataset
                            {
                                BackgroundColor = new[] {"rgba(75, 192, 192, 0.0)", "rgba(54, 162, 235, 0.2)", "rgba(255, 99, 132, 0.0)" },
                                BorderColor = new[] { "rgba(102, 102, 102, 1)", "rgba(54, 162, 235, 1)", "rgba(102, 102, 102, 1)" },
                                BorderWidth = 1,
                           }
                        },
                    Labels = new[] { " ", "avg", " " },
                };

                // Dummy data 
                data.Datasets[0].Data = new List<double> { 65, 10, 25 };

                chart.Data = data;
                return chart;
            });
           
            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            ViewData["percentage"] = "76%";
            return View($"ProductLeadTime");
        }
    }
}
