using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.ReportingModel;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public partial class MergedMachineWorkloadViewComponent : ViewComponent
    {
        private readonly ResultContext _context;
        private List<Tuple<int, SimulationType>> _simList;
        
        public MergedMachineWorkloadViewComponent(ResultContext context)
        {
            _context = context;
            _simList = new List<Tuple<int, SimulationType>>();
        }


        /// <summary>
        /// 1st = Param[0] = SimulationType
        /// 2nd = Param[1] = SimulationId 1
        /// 3rd = Param[2] = SimulationType
        /// 4th = Param[3] = SimulationId 2
        /// 5th = Param[4] = SimulationType
        /// 6th = Param[5] = SimulationId 3
        /// 7th = Param[6] = SimulationType
        /// 8th = Param[7] = SimulationId 4
        /// </summary>
        /// <param name="paramsList"></param>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            Task<Chart> generateChartTask = GenerateChartTask(paramsList: paramsList);
            _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 0]), item2: (paramsList[index: 1] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() == 8) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 6]), item2: (paramsList[index: 7] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 6) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 4]), item2: (paramsList[index: 5] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 4) _simList.Add(item: new Tuple<int, SimulationType>(item1: Convert.ToInt32(value: paramsList[index: 2]), item2: (paramsList[index: 3] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            


            // create JS to Render Chart.
            ViewData[index: "chart"] = await generateChartTask;
            return View(viewName: $"MergedMachineWorkload");

        }
        private Task<Chart> GenerateChartTask(List<string> paramsList)
        {
            var generateChartTask = Task.Run(function: () =>
            {
                if (!_context.SimulationOperations.Any())
                {
                    return null;
                }

                Chart chart = new Chart
                {
                    Type = Enums.ChartType.Bar,
                    Options = new Options { MaintainAspectRatio = true }
                };

                var machines = new List<Kpi>();
                // charttype
                foreach (var sim in _simList)
                {
                    var trick17 = _context.Kpis.Where(predicate: x => x.SimulationConfigurationId == sim.Item1
                                                 && x.KpiType == KpiType.MachineUtilization
                                                 && x.IsKpi && x.SimulationType == sim.Item2
                                                 && x.SimulationNumber == 1
                                                 && x.IsFinal).OrderByDescending(keySelector: g => g.Name);
                    machines.AddRange(collection: trick17.ToList());
                }




                var data = new Data { Labels = machines.Select(selector: n => n.Name).Distinct().ToList() };

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                var cc = new ChartColors();
                
                //var max = _context.SimulationWorkschedules.Max(x => x.End) - 1440; 
                foreach (var t1 in _simList.OrderBy(keySelector: x => x.Item1))
                {
                    var barDataSet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<ChartColor>(), HoverBackgroundColor = new List<ChartColor>(), YAxisID = "y-normal" };
                    var barDiversityInvisSet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<ChartColor>(), HoverBackgroundColor = new List<ChartColor>(), YAxisID = "y-diversity" };
                    var barDiversitySet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<ChartColor>(), HoverBackgroundColor = new List<ChartColor>(), YAxisID = "y-diversity" };
                    barDataSet.Label = "Sim Id:" + t1.Item1 + " " + t1.Item2;
                    foreach (var machineName in data.Labels)
                    {

                        Kpi machine = null;
                        var t  = machines.Where(predicate: x => x.Name == machineName && x.SimulationConfigurationId == t1.Item1 && x.SimulationType == t1.Item2).Distinct();
                        machine = t.Single();

                        var percent = Math.Round(value: machine.Value * 100, digits: 2);
                        // var wait = max - work;
                        barDataSet.Data.Add(item: percent);
                        barDiversitySet.BackgroundColor.Add(item: cc.Get(i, 0.4));
                        barDiversitySet.HoverBackgroundColor.Add(item: cc.Get(i, 0.7));

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
                    i++;
                    data.Datasets.Add(item: barDataSet);
                    //data.Datasets.Add(barDiversityInvisSet);
                    //data.Datasets.Add(barDiversitySet);
                }
                
                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() { new CartesianScale { Stacked = false, Id = "x-normal", Display = true } };
                var yAxis = new List<Scale>()
                {
                    new CartesianScale { Stacked = false, Display = true, Ticks = new CartesianLinearTick {BeginAtZero = true, Min = 0, Max = 100}, Id = "y-normal" },
                    new CartesianScale {
                        Stacked = false, Ticks = new CartesianLinearTick {BeginAtZero = true, Min = 0, Max = 100}, Display = false,
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
