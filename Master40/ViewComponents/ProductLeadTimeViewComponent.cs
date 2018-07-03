using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;
using Master40.DB.Enums;
using Master40.DB.Models;

namespace Master40.ViewComponents
{
    public partial class ProductLeadTimeViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private List<Tuple<int, SimulationType>> _simList;

        public ProductLeadTimeViewComponent(ProductionDomainContext context)
        {
            _simList = new List<Tuple<int, SimulationType>>();
            _context = context;
        }

        private string BoxplotCallback()
        {
            return @"function(tooltipItem, data) {
        		                    var text = '';
                                  switch (tooltipItem.datasetIndex){
              	                    case 0:
                	                    text = 'Min: ' + data.datasets[0].data[0];

                                        break; 
              	                    case 1:
                                      text = '2. Quantile: ' + Math.round(data.datasets[0].data[0] + data.datasets[1].data[0] + data.datasets[2].data[0]);
                                break; 
              	                    case 3:
                	                    text = 'Median: ' + Math.round(data.datasets[0].data[0] + data.datasets[1].data[0] + data.datasets[2].data[0] + data.datasets[3].data[0]);
                                break; 
              	                    case 4:
                	                    text = '3. Quantile: ' + Math.round(data.datasets[0].data[0] + data.datasets[1].data[0] + data.datasets[2].data[0] + data.datasets[3].data[0] + data.datasets[4].data[0]);
                                break;   
              	                    case 6:
                	                    text = 'Max: ' + Math.round(data.datasets[0].data[0] + data.datasets[1].data[0] + data.datasets[2].data[0] + data.datasets[3].data[0] + data.datasets[4].data[0] + data.datasets[5].data[0] + data.datasets[6].data[0]);
                                break;
                                default:
                	                    text = '';
                            }
                                  return text;              
                              }";
        }


        /// <summary>
        /// 1st = Param[0] = SimulationId
        /// 2st = Param[1] = SimulationType
        /// 3nd = Param[2] = SimulationNumber
        /// </summary>
        /// <param name="paramsList"></param>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            // Determine Type and Data
            _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[0]), (paramsList[1] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() == 8) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[6]), (paramsList[7] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 6) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[4]), (paramsList[5] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 4) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[2]), (paramsList[3] == "Central") ? SimulationType.Central : SimulationType.Decentral));

            var kpi = new List<Kpi>();
            // charttype
            foreach (var sim in _simList)
            {
                var trick17 = _context.Kpis.Where(x => x.KpiType == KpiType.LeadTime
                                                       && x.SimulationConfigurationId == sim.Item1
                                                       && x.SimulationNumber == 1
                                                       && x.SimulationType == sim.Item2);
                kpi.AddRange(trick17.ToList());
            }

            var max = kpi.Max(m => m.Value);

            var generateChartTask = Task.Run(() =>
            {
                if (!_context.SimulationWorkschedules.Any())
                {
                    return null;
                }


                Chart chart = new Chart();

                // charttype
                chart.Type = "bar";

                // use available hight in Chart
                chart.Options = new Options()
                {
                    MaintainAspectRatio = false,
                    Responsive = true,
                    Legend = new Legend { Position = "bottom", Display = false },
                    Title = new Title { Text = "BoxPlot LeadTimes", Position = "top", FontSize = 24, FontStyle = "bold" , Display = true },
                    Scales = new Scales { YAxes = new List<Scale> { new CartesianScale { Stacked = true, Display = true, Ticks = new CartesianLinearTick { Max = ((int)Math.Ceiling(max / 100.0)) * 100 } } },
                                          XAxes = new List<Scale> { new CartesianScale { Stacked = true , Display = true} },
                    },
                    Tooltips = new ToolTip { Mode = "x", Callbacks = new Callback { Label = BoxplotCallback() } }
                };

                var labels = kpi.Select(n => n.Name).Distinct().ToList();

                var data = new Data
                {
                    Datasets = new List<Dataset>(),
                    Labels = labels,
                };
                var dsClear = new BarDataset { Data = new List<double>(), Label = "dsClear", BackgroundColor = new List<string>(), BorderWidth = new List<int>(), BorderColor = new List<string>() };
                var lowerStroke = new BarDataset { Data = new List<double>(), Label = "lowerStroke", BackgroundColor = new List<string>(), BorderWidth = new List<int>(), BorderColor = new List<string>() };
                var firstQuartile = new BarDataset { Data = new List<double>(), Label = "fQ", BackgroundColor = new List<string>(), BorderWidth = new List<int>(), BorderColor = new List<string>() };
                var secondQuartile = new BarDataset { Data = new List<double>(), Label = "Med", BackgroundColor = new List<string>(), BorderWidth = new List<int>(), BorderColor = new List<string>() };
                var thirdQuartile = new BarDataset { Data = new List<double>(), Label = "uQ", BackgroundColor = new List<string>(), BorderWidth = new List<int>(), BorderColor = new List<string>() };
                var fourthQuartile = new BarDataset { Data = new List<double>(), Label = "line", BackgroundColor = new List<string>(), BorderWidth = new List<int>(), BorderColor = new List<string>() };
                var upperStroke = new BarDataset { Data = new List<double>(), Label = "upperStroke", BackgroundColor = new List<string>(), BorderWidth = new List<int>(), BorderColor = new List<string>() };


                var products = kpi.Select(x => x.Name).Distinct().ToList();
                var colors = new ChartColor();
                int i = 0;

                foreach (var sim in _simList)
                {
                    foreach (var product in products)
                    {
                        var boxplotValues = kpi.Where(x => x.IsKpi == false && x.Name == product 
                                                        && x.SimulationConfigurationId == sim.Item1
                                                        && x.SimulationType == sim.Item2).OrderBy(x => x.Value)
                            .ToList();

                        dsClear.Data.Add((double) boxplotValues.ElementAt(0).Value);
                        dsClear.BackgroundColor.Add(ChartColor.Transparent);
                        dsClear.BorderColor.Add(ChartColor.Transparent);
                        dsClear.BorderWidth.Add(0);

                        lowerStroke.Data.Add(5);
                        lowerStroke.BackgroundColor.Add(ChartColor.Transparent);
                        lowerStroke.BorderColor.Add("rgba(50, 50, 50, 1)");
                        lowerStroke.BorderWidth.Add(2);

                        var fq = (double) (boxplotValues.ElementAt(1).Value - boxplotValues.ElementAt(0).Value - 5);
                        firstQuartile.Data.Add(fq);
                        firstQuartile.BackgroundColor.Add("rgba(50, 50, 50, 1)");// .Add(colors.Color[i].Substring(0, colors.Color[i].Length - 4) + "0.8)");
                        firstQuartile.BorderColor.Add("rgba(50, 50, 50, 1)");
                        firstQuartile.BorderWidth.Add(0);

                        var m = (double) (boxplotValues.ElementAt(2).Value - boxplotValues.ElementAt(1).Value);
                        secondQuartile.Data.Add(m);
                        secondQuartile.BackgroundColor.Add(colors.Color[i].Substring(0, colors.Color[i].Length - 4) +
                                                           "0.8)");
                        secondQuartile.BorderColor.Add("rgba(50, 50, 50, 1)");
                        secondQuartile.BorderWidth.Add(1);

                        var up = (double) (boxplotValues.ElementAt(3).Value - boxplotValues.ElementAt(2).Value);
                        thirdQuartile.Data.Add(up);
                        thirdQuartile.BackgroundColor.Add(colors.Color[i].Substring(0, colors.Color[i].Length - 4) +
                                                          "0.8)");
                        thirdQuartile.BorderColor.Add("rgba(50, 50, 50, 1)");
                        thirdQuartile.BorderWidth.Add(1);

                        var hs = (double) (boxplotValues.ElementAt(4).Value - boxplotValues.ElementAt(3).Value - 5);
                        fourthQuartile.Data.Add(hs);
                        fourthQuartile.BackgroundColor.Add("rgba(50, 50, 50, 1)"); //.Add(colors.Color[i].Substring(0, colors.Color[i].Length - 4) +  "0.8)");
                        fourthQuartile.BorderColor.Add("rgba(50, 50, 50, 1)");
                        fourthQuartile.BorderWidth.Add(0);

                        upperStroke.Data.Add(5);
                        upperStroke.BackgroundColor.Add(ChartColor.Transparent);
                        upperStroke.BorderColor.Add("rgba(50, 50, 50, 1)");
                        upperStroke.BorderWidth.Add(2);
                        i = i + 2;
                    }

                }


                data.Datasets.Add(dsClear);
                data.Datasets.Add(lowerStroke);
                data.Datasets.Add(firstQuartile);
                data.Datasets.Add(secondQuartile);
                data.Datasets.Add(thirdQuartile);
                data.Datasets.Add(fourthQuartile);
                data.Datasets.Add(upperStroke);

                var avg = kpi.Sum(x => x.Value) / kpi.Count();
                var min = kpi.Min(x => x.Value);
                var end = ((int)Math.Ceiling(max / 100.0)) * 100;


                //data.Datasets[0].Data = new List<double> { 0, (int)(min/end*100), (int)(avg /end*100), (int)(max /end*100), end };
                //data.Datasets[0].Data = new List<double> { min, avg, 10, max, end-max };

                chart.Data = data;
                return chart;
            });
           
            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            ViewData["Data"] = kpi.Where(w => w.IsFinal && w.IsKpi).ToList();
            ViewData["percentage"] = Math.Round(kpi.Sum(x => x.Value) / kpi.Count(), 0);
            return View($"ProductLeadTime");
        }
    }
}
