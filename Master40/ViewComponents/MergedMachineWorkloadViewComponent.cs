using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;
using Master40.DB.Enums;
using ChartJSCore.Models.Bar;
using Master40.DB.Models;

namespace Master40.ViewComponents
{
    public partial class MergedMachineWorkloadViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public MergedMachineWorkloadViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }


        /// <summary>
        /// 1st = Param[0] = SimulationId
        /// 2nd = Param[1] = SimulationNumber
        /// </summary>
        /// <param name="paramsList"></param>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            Task<Chart> generateChartTask = GenerateChartTask(paramsList);

            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            return View($"MergedMachineWorkload");

        }
        private Task<Chart> GenerateChartTask(List<string> paramsList)
        {
            var generateChartTask = Task.Run(() =>
            {
                if (!_context.SimulationWorkschedules.Any())
                {
                    return null;
                }

                Chart chart = new Chart
                {
                    Type = "bar",
                    Options = new Options { MaintainAspectRatio = true }
                };

                // charttype

                // use available hight in Chart
                // use available hight in Chart
                var machines = _context.Kpis.Where(x => x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                                        && x.KpiType == KpiType.MachineUtilization
                                                        && x.IsKpi
                                                        && x.IsFinal && x.SimulationNumber == Convert.ToInt32(paramsList[1]))
                                           .OrderByDescending(g => g.Name).ToList();


                var data = new Data { Labels = machines.Select(n => n.Name).Distinct().ToList() };

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                var cc = new ChartColor();

                //var max = _context.SimulationWorkschedules.Max(x => x.End) - 1440; 
                for (double t = 0.4; t < 0.8; t = t + 0.3)
                {
                    var barDataSet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<string>(), HoverBackgroundColor = new List<string>(), YAxisID = "y-normal" };
                    var barDiversityInvisSet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<string>(), HoverBackgroundColor = new List<string>(), YAxisID = "y-diversity" };
                    var barDiversitySet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<string>(), HoverBackgroundColor = new List<string>(), YAxisID = "y-diversity" };
                    if (t == 0.4) barDataSet.Label = "Dentral";
                    else barDataSet.Label = "Central";
                    foreach (var machineName in data.Labels)
                        {

                            Kpi machine = null;
                            if (t == 0.4) machine = machines.Single(x => x.Name == machineName && x.SimulationType == SimulationType.Decentral);
                            else machine = machines.Single(x => x.Name == machineName && x.SimulationType == SimulationType.Central);

                            var percent = Math.Round(machine.Value * 100, 2);
                            // var wait = max - work;
                            barDataSet.Data.Add(percent);
                            barDataSet.BackgroundColor.Add(cc.Color[i].Substring(0, cc.Color[1].Length - 4) + t.ToString().Replace(",", ".") + ")");// "0.4)");
                            barDataSet.HoverBackgroundColor.Add(cc.Color[i].Substring(0, cc.Color[1].Length - 4) + (t + 0.3).ToString().Replace(",", ".") + ")"); //"0.7)");

                            //var varianz = machine.Count * 100;

                            //barDiversityInvisSet.Data.Add(percent - Math.Round(varianz / 2, 2));
                            //barDiversityInvisSet.BackgroundColor.Add(ChartColor.Transparent);
                            //barDiversityInvisSet.HoverBackgroundColor.Add(ChartColor.Transparent);
                            //
                            //barDiversitySet.Data.Add(Math.Round(varianz, 2));
                            //barDiversitySet.BackgroundColor.Add(cc.Color[i].Substring(0, cc.Color[1].Length - 4) + (t + 0.3) + ")");
                            //barDiversitySet.HoverBackgroundColor.Add(cc.Color[i].Substring(0, cc.Color[1].Length - 4) + "1)");


                        }
                    i++;
                    data.Datasets.Add(barDataSet);
                    //data.Datasets.Add(barDiversityInvisSet);
                    //data.Datasets.Add(barDiversitySet);

                }
                
                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() { new BarScale { Stacked = false, Id = "x-normal", Display = true } };
                var yAxis = new List<Scale>()
                {
                    new BarScale { Stacked = false, Display = true, Ticks = new Tick {BeginAtZero = true, Min = 0, Max = 100}, Id = "y-normal" },
                    new BarScale {
                        Stacked = false, Ticks = new Tick {BeginAtZero = true, Min = 0, Max = 100}, Display = false,
                        Id = "y-diversity", ScaleLabel = new ScaleLabel{ LabelString = "Value in %", Display = false, FontSize = 12 },
                    },
                };
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options()
                {
                    Scales = new Scales { XAxes = xAxis, YAxes = yAxis },
                    MaintainAspectRatio = false,
                    Responsive = true,
                    Legend = new Legend { Display = false }
                };

                return chart;
            });
            return generateChartTask;
        }
    }
}
