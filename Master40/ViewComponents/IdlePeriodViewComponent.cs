using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using Master40.DB.Enums;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public class IdlePeriodViewComponent : ViewComponent
    {
        private readonly ResultContext _context;

        public IdlePeriodViewComponent(ResultContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            SimulationType simType = (paramsList[index: 1].Equals(value: "Decentral")) ? SimulationType.Decentral : SimulationType.Central;
            var kpis = _context.Kpis.Where(predicate: x => x.KpiType == KpiType.LayTime
                                               && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                               && x.SimulationType == simType
                                               && x.SimulationNumber == Convert.ToInt32(paramsList[2])
                                               && x.IsKpi == true).OrderBy(keySelector: x => x.Name).ToList();
            var maxVal = _context.Kpis.Where(predicate: x => x.KpiType == KpiType.LayTime
                                                && x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                                && x.SimulationNumber == Convert.ToInt32(paramsList[2])
                                                && x.IsKpi == true).Max(selector: x => x.ValueMax);
            maxVal = Math.Ceiling(a: maxVal / 100) * 100;

            var generateChartTask = Task.Run(function: () =>
            {
                if (!_context.SimulationOperations.Any())
                {
                    return null;
                }

                Chart chart = new Chart();
                var lables = new List<string>();
                var cc = new ChartColors();
                // charttype
                chart.Type = Enums.ChartType.HorizontalBar;

                var data = new Data { Datasets = new List<Dataset>() };

                var min = new BarDataset {
                    Type = Enums.ChartType.HorizontalBar,
                    Data = new List<double>(),
                    BackgroundColor = new List<ChartColor>() // { cc.Color[8], cc.Color[4], cc.Color[1] } 
                };
                var avg = new BarDataset {
                    Type = Enums.ChartType.HorizontalBar,
                    Data = new List<double>(),
                    BackgroundColor = new List<ChartColor>() // { cc.Color[8], , cc.Color[1] }
                };
                var max = new BarDataset {
                    Type = Enums.ChartType.HorizontalBar,
                    Data = new List<double>(),
                    BackgroundColor = new List<ChartColor>() // { cc.Color[8], cc.Color[4], cc.Color[1] }
                };

                foreach (var kpi in kpis)
                {
                    lables.Add(item: kpi.Name);
                    min.Data.Add(item: kpi.ValueMin);
                    avg.Data.Add(item: kpi.Value - kpi.ValueMin);
                    max.Data.Add(item: kpi.ValueMax - kpi.Value);
                    min.BackgroundColor.Add(item: ChartColors.Transparent);
                    avg.BackgroundColor.Add(item: cc.Color[index: 4]);
                    max.BackgroundColor.Add(item: cc.Color[index: 1]);
                }

                data.Datasets.Add(item: min);
                data.Datasets.Add(item: avg);
                data.Datasets.Add(item: max);
                data.Labels = lables;

                var xAxis = new List<Scale>() { new CartesianScale
                {
                    Stacked = true, Display = true, Ticks = new CartesianLinearTick { Max = Convert.ToInt32(value: maxVal), Display = true } , 
                    Id = "first-x-axis", Type = "linear", ScaleLabel = new ScaleLabel { LabelString = "Time in min", Display = true, FontSize = 12 }
                }, };
                var yAxis = new List<Scale>() { new CartesianScale { Stacked = true, Display = true } }; // Ticks = new Tick { BeginAtZero = true, Min = 0, Max = 100 }
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options() { Scales = new Scales { XAxes = xAxis, YAxes = yAxis }, MaintainAspectRatio = false, Responsive = true, Legend = new Legend { Display = false } };
                chart.Data = data;
                return chart;
            });

            // create JS to Render Chart.
            ViewData[index: "chart"] = await generateChartTask;
            ViewData[index: "Type"] = paramsList[index: 1];
            ViewData[index: "Data"] = kpis.ToList();
            return View(viewName: $"IdlePeriod");
        }
    }
}
