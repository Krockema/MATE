using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.Extensions;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;

namespace Master40.ViewComponents
{
    public partial class ProductLeadTimeViewComponent : ViewComponent
    {
        private readonly ResultContext _context;
        private List<Tuple<int, SimulationType>> _simList;

        public ProductLeadTimeViewComponent(ResultContext context)
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
            _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 0]), item2: (paramsList[index: 1] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() == 8) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 6]), item2: (paramsList[index: 7] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 6) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 4]), item2: (paramsList[index: 5] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 4) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 2]), item2: (paramsList[index: 3] == "Central") ? SimulationType.Central : SimulationType.Decentral));

            var kpi = new List<Kpi>();
            // charttype
            foreach (var sim in _simList)
            {
                var trick17 = _context.Kpis.Where(predicate: x => x.KpiType == KpiType.LeadTime
                                                       && x.SimulationConfigurationId == sim.Item1
                                                       && x.SimulationNumber == 1
                                                       && x.SimulationType == sim.Item2);
                kpi.AddRange(collection: trick17.ToList());
            }

            var max = kpi.Max(selector: m => m.Value);

            var generateChartTask = Task.Run(function: () =>
            {
                if (!_context.SimulationOperations.Any())
                {
                    return null;
                }


                Chart chart = new Chart();

                // charttype
                chart.Type = Enums.ChartType.Bar;

                // use available hight in Chart
                chart.Options = new Options()
                {
                    MaintainAspectRatio = false,
                    Responsive = true,
                    Legend = new Legend { Position = "bottom", Display = false },
                    Title = new Title { Text = "BoxPlot LeadTimes", Position = "top", FontSize = 24, FontStyle = "bold" , Display = true },
                    Scales = new Scales { YAxes = new List<Scale> { new CartesianScale { Stacked = true, Display = true, Ticks = new CartesianLinearTick { Max = ((int)Math.Ceiling(a: max / 100.0)) * 100 } } },
                                          XAxes = new List<Scale> { new CartesianScale { Stacked = true , Display = true} },
                    },
                    Tooltips = new ToolTip { Mode = "x", Callbacks = new Callback { Label = BoxplotCallback() } }
                };

                var labels = kpi.Select(selector: n => n.Name).Distinct().ToList();

                var data = new Data
                {
                    Datasets = new List<Dataset>(),
                    Labels = labels,
                };
                var dsClear = new BarDataset { Data = new List<double>(), Label = "dsClear", BackgroundColor = new List<ChartColor>(), BorderWidth = new List<int>(), BorderColor = new List<ChartColor>() };
                var lowerStroke = new BarDataset { Data = new List<double>(), Label = "lowerStroke", BackgroundColor = new List<ChartColor>(), BorderWidth = new List<int>(), BorderColor = new List<ChartColor>() };
                var firstQuartile = new BarDataset { Data = new List<double>(), Label = "fQ", BackgroundColor = new List<ChartColor>(), BorderWidth = new List<int>(), BorderColor = new List<ChartColor>() };
                var secondQuartile = new BarDataset { Data = new List<double>(), Label = "Med", BackgroundColor = new List<ChartColor>(), BorderWidth = new List<int>(), BorderColor = new List<ChartColor>() };
                var thirdQuartile = new BarDataset { Data = new List<double>(), Label = "uQ", BackgroundColor = new List<ChartColor>(), BorderWidth = new List<int>(), BorderColor = new List<ChartColor>() };
                var fourthQuartile = new BarDataset { Data = new List<double>(), Label = "line", BackgroundColor = new List<ChartColor>(), BorderWidth = new List<int>(), BorderColor = new List<ChartColor>() };
                var upperStroke = new BarDataset { Data = new List<double>(), Label = "upperStroke", BackgroundColor = new List<ChartColor>(), BorderWidth = new List<int>(), BorderColor = new List<ChartColor>() };


                var products = kpi.Select(selector: x => x.Name).Distinct().ToList();
                var colors = new ChartColors();
                int i = 0;

                foreach (var sim in _simList)
                {
                    foreach (var product in products)
                    {
                        var boxplotValues = kpi.Where(predicate: x => x.IsKpi == false && x.Name == product 
                                                        && x.SimulationConfigurationId == sim.Item1
                                                        && x.SimulationType == sim.Item2).OrderBy(keySelector: x => x.Value)
                            .ToList();

                        dsClear.Data.Add(item: (double) boxplotValues.ElementAt(index: 0).Value);
                        dsClear.BackgroundColor.Add(item: ChartColors.Transparent);
                        dsClear.BorderColor.Add(item: ChartColors.Transparent);
                        dsClear.BorderWidth.Add(item: 0);

                        lowerStroke.Data.Add(item: 5);
                        lowerStroke.BackgroundColor.Add(item: ChartColors.Transparent);
                        lowerStroke.BorderColor.Add(item: ChartColor.FromRgba(50, 50, 50, 1));
                        lowerStroke.BorderWidth.Add(item: 2);

                        var fq = (double) (boxplotValues.ElementAt(index: 1).Value - boxplotValues.ElementAt(index: 0).Value - 5);
                        firstQuartile.Data.Add(item: fq);
                        firstQuartile.BackgroundColor.Add(item: ChartColor.FromRgba(50, 50, 50, 1));// .Add(colors.Color[i].Substring(0, colors.Color[i].Length - 4) + "0.8)");
                        firstQuartile.BorderColor.Add(item: ChartColor.FromRgba(50, 50, 50, 1));
                        firstQuartile.BorderWidth.Add(item: 0);

                        var m = (double) (boxplotValues.ElementAt(index: 2).Value - boxplotValues.ElementAt(index: 1).Value);
                        secondQuartile.Data.Add(item: m);
                        secondQuartile.BackgroundColor.Add(item: colors.Get(i, 0.8));
                        secondQuartile.BorderColor.Add(item: ChartColor.FromRgba(50, 50, 50, 1));
                        secondQuartile.BorderWidth.Add(item: 1);

                        var up = (double) (boxplotValues.ElementAt(index: 3).Value - boxplotValues.ElementAt(index: 2).Value);
                        thirdQuartile.Data.Add(item: up);
                        thirdQuartile.BackgroundColor.Add(item: colors.Get(i, 0.8));
                        thirdQuartile.BorderColor.Add(item: ChartColor.FromRgba(50, 50, 50, 1));
                        thirdQuartile.BorderWidth.Add(item: 1);

                        var hs = (double) (boxplotValues.ElementAt(index: 4).Value - boxplotValues.ElementAt(index: 3).Value - 5);
                        fourthQuartile.Data.Add(item: hs);
                        fourthQuartile.BackgroundColor.Add(item: ChartColor.FromRgba(50, 50, 50, 1)); //.Add(colors.Color[i].Substring(0, colors.Color[i].Length - 4) +  "0.8)");
                        fourthQuartile.BorderColor.Add(item: ChartColor.FromRgba(50, 50, 50, 1));
                        fourthQuartile.BorderWidth.Add(item: 0);

                        upperStroke.Data.Add(item: 5);
                        upperStroke.BackgroundColor.Add(item: ChartColors.Transparent);
                        upperStroke.BorderColor.Add(item: ChartColor.FromRgba(50, 50, 50, 1));
                        upperStroke.BorderWidth.Add(item: 2);
                        i = i + 2;
                    }

                }


                data.Datasets.Add(item: dsClear);
                data.Datasets.Add(item: lowerStroke);
                data.Datasets.Add(item: firstQuartile);
                data.Datasets.Add(item: secondQuartile);
                data.Datasets.Add(item: thirdQuartile);
                data.Datasets.Add(item: fourthQuartile);
                data.Datasets.Add(item: upperStroke);

                var avg = kpi.Sum(selector: x => x.Value) / kpi.Count();
                var min = kpi.Min(selector: x => x.Value);
                var end = ((int)Math.Ceiling(a: max / 100.0)) * 100;


                //data.Datasets[0].Data = new List<double> { 0, (int)(min/end*100), (int)(avg /end*100), (int)(max /end*100), end };
                //data.Datasets[0].Data = new List<double> { min, avg, 10, max, end-max };

                chart.Data = data;
                return chart;
            });
           
            // create JS to Render Chart.
            ViewData[index: "chart"] = await generateChartTask;
            ViewData[index: "Type"] = paramsList[index: 1];
            ViewData[index: "Data"] = kpi.Where(predicate: w => w.IsFinal && w.IsKpi).ToList();
            ViewData[index: "percentage"] = Math.Round(value: kpi.Sum(selector: x => x.Value) / kpi.Count(), digits: 0);
            return View(viewName: $"ProductLeadTime");
        }
    }
}
