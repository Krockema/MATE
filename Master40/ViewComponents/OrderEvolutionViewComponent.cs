using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Enums;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public class OrderEvolutionViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public OrderEvolutionViewComponent(ProductionDomainContext context)
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
                ChartColor cc = new ChartColor();
                // charttype
                chart.Type = "scatter";
                var simConfig = _context.SimulationConfigurations.Single(a => a.Id == Convert.ToInt32(paramsList[0]));
                // use available hight in Chart
                var maxY = Math.Floor((decimal)simConfig.SimulationEndTime / 1000) * 1000;
                var maxX = 100;
                chart.Options = new LineOptions()
                {
                    MaintainAspectRatio = false,
                    Responsive = true,
                    Scales = new Scales
                    {
                        YAxes = new List<Scale> { new CartesianScale { Id = "first-y-axis", Type = "linear", Display = true
                                                , Ticks = new CartesianLinearTick{ Max = maxX, Min = 0 , Display = true }
                                                , ScaleLabel = new ScaleLabel { LabelString = "Quantity", Display = true, FontSize = 12 } } },
                        XAxes = new List<Scale> { new CartesianScale { Id = "first-x-axis", Type = "linear", Display = true
                                                , Ticks = new CartesianLinearTick{ Max = Convert.ToInt32(maxY), Min = 0, Display = true }
                                                , ScaleLabel = new ScaleLabel { LabelString = "Time in min", Display = true, FontSize = 12 } } }
                    },
                    Legend = new Legend { Position = "bottom", Display = true, FullWidth = true },
                    Title = new Title { Text = "Order Evolution", Position = "top", FontSize = 24, FontStyle = "bold" }
                };


                SimulationType simType = (paramsList[1].Equals("Decentral")) ? SimulationType.Decentral : SimulationType.Central;
                var kpis = _context.SimulationOrders.Where(x => x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                                   && x.SimulationNumber == Convert.ToInt32(paramsList[2])
                                                   && x.SimulationType == simType
                                                   && x.FinishingTime != 0); // filter unfinished orders

                var data = new Data { Datasets = new List<Dataset>() };
                
                    var startVal = 0;
                    var ts = simConfig.DynamicKpiTimeSpan;
                    var input = new List<LineScatterData> { new LineScatterData { x = "0", y = "0" } };
                    var progress = new List<LineScatterData> { new LineScatterData { x = "0", y = "0" } };
                    var output = new List<LineScatterData> { new LineScatterData { x = "0", y = "0" } };
                    for (var i = ts; i < simConfig.SimulationEndTime; i = i + ts)
                    {
                            input.AddRange(kpis.Where(x => x.CreationTime >= startVal
                                                        && x.CreationTime < i)
                                .Select(x => new { time = i , value = (decimal)x.Name.Count() })
                                .GroupBy(g => g.time)
                                .Select(n => new LineScatterData { x = Convert.ToDouble(n.Key).ToString(), y = Convert.ToDouble(n.Count()).ToString() }).ToList());

                            progress.AddRange(kpis.Where(x => x.CreationTime <= i
                                                      && x.FinishingTime > i)
                                .Select(x => new { time = i, value = x.Name.Count() })
                                .GroupBy(g => g.time)
                                .Select(n => new LineScatterData { x = Convert.ToDouble(n.Key).ToString(), y = Convert.ToDouble(n.Count()).ToString() }).ToList());

                            output.AddRange(kpis.Where(x => x.FinishingTime >= startVal
                                                      && x.FinishingTime < i)
                                .Select(x => new { time = i, value = x.Name.Count() })
                                .GroupBy(g => g.time)
                                .Select(n => new LineScatterData { x = Convert.ToDouble(n.Key).ToString(), y = Convert.ToDouble(n.Count()).ToString() }).ToList());
                            startVal = i;
                        }

                data.Datasets.Add(new LineScatterDataset()
                {
                    Data = new List<LineScatterData> { new LineScatterData { x = "0", y = maxX.ToString() }, new LineScatterData { x = simConfig.SettlingStart.ToString(), y = maxX.ToString() } },
                    BorderWidth = 1,
                    Label = "Settling time",
                    BackgroundColor = "rgba(0, 0, 0, 0.1)",
                    BorderColor = "rgba(0, 0, 0, 0.3)",
                    ShowLine = true,
                    Fill = "true",
                    //SteppedLine = false,
                    LineTension = 0,
                    PointRadius = new List<int> { 0, 0 }
                });

                data.Datasets.Add(new LineScatterDataset()
                    {
                        Data = input,
                        BorderWidth = 3,
                        Label = "Input",
                        BackgroundColor = cc.Color[3],
                        BorderColor = cc.Color[3],
                        ShowLine = true,
                        Fill = "false",
                        //SteppedLine = false,
                        LineTension = 0.5
                    });
                    
                    data.Datasets.Add(new LineScatterDataset()
                    {
                        Data = progress,
                        BorderWidth = 3,
                        Label = "Processing",
                        BackgroundColor = cc.Color[0],
                        BorderColor = cc.Color[0],
                        ShowLine = true,
                        Fill = "false",
                        //SteppedLine = false,
                        LineTension = 0.5
                    });
                    
                    data.Datasets.Add(new LineScatterDataset()
                    {
                        Data = output,
                        BorderWidth = 3,
                        Label = "Output",
                        BackgroundColor = cc.Color[2],
                        BorderColor = cc.Color[2],
                        Fill = "false",
                        ShowLine = true,
                        //SteppedLine = false,
                        LineTension = 0.5
                    });
                


                chart.Data = data;
                return chart;
            });

            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            return View($"OrderEvolution");
        }
    }
}
