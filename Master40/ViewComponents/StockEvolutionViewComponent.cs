using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                Chart chart = new Chart();

                // charttype
                chart.Type = "line";

                // use available hight in Chart
                chart.Options = new LineOptions
                {
                    MaintainAspectRatio = false,
                    Responsive = true,
                    
                    Legend = new Legend { Position = "right", Display = true },
                    Title = new Title { Text = "Stock Evolution", Position = "top", FontSize = 24, FontStyle = "bold" }
                };
                var data = new Data
                {
                    Datasets = new List<Dataset>
                        {
                            new LineDataset
                            {
                                // stock Values over time
                                Data = new List<double>{ 0.0, 10.0, 8.0, 6.0, 4.0, 2.0, 10.0, 8.0, 6.0, 4.0, 2.0, 10.0 },
                                BorderWidth = 1,
                                Label = "Plugs"
                            },
                            new LineDataset
                            {
                                // stock Values over time
                                Data = new List<double>{ 2.0, 8.0, 4.0, 0.0, 10.0, 6.0, 2.0, 10.0, 6.0, 4.0, 2.0, 10.0 },
                                BorderWidth = 1,
                                Label = "Wheels"
                            },
                            new LineDataset
                            {
                                // min Stock
                                Data = new List<double>{ 4,4,4,4,4,4,4,4,4,4,4,4 },
                                BorderWidth = 1,
                                BorderColor = "Red",
                                Label = "Minimum Stock"
                            }
                        },
                   
                   Labels = new[] { "0", "1", "2", "3", "4", "5", "2", "7", "8", "9", "10", "11" },
                };

                // Dummy data 
               // data.Datasets[0].Data = new List<double> { 65, 10, 25 };

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
