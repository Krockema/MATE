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
        private List<Tuple<int, SimulationType>> _simList;
        
        public MergedMachineWorkloadViewComponent(ProductionDomainContext context)
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
            Task<Chart> generateChartTask = GenerateChartTask(paramsList);
            _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[0]), (paramsList[1] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() == 8) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[6]), (paramsList[7] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 6) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[4]), (paramsList[5] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            if (paramsList.Count() >= 4) _simList.Add(new Tuple<int, SimulationType>(Convert.ToInt32(paramsList[2]), (paramsList[3] == "Central") ? SimulationType.Central : SimulationType.Decentral));
            


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

                var machines = new List<Kpi>();
                // charttype
                foreach (var sim in _simList)
                {
                    var trick17 = _context.Kpis.Where(x => x.SimulationConfigurationId == sim.Item1
                                                 && x.KpiType == KpiType.MachineUtilization
                                                 && x.IsKpi && x.SimulationType == sim.Item2
                                                 && x.SimulationNumber == 1
                                                 && x.IsFinal).OrderByDescending(g => g.Name);
                    machines.AddRange(trick17.ToList());
                }




                var data = new Data { Labels = machines.Select(n => n.Name).Distinct().ToList() };

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                var cc = new ChartColor();
                
                //var max = _context.SimulationWorkschedules.Max(x => x.End) - 1440; 
                foreach (var t1 in _simList.OrderBy(x => x.Item1))
                {
                    var barDataSet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<string>(), HoverBackgroundColor = new List<string>(), YAxisID = "y-normal" };
                    var barDiversityInvisSet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<string>(), HoverBackgroundColor = new List<string>(), YAxisID = "y-diversity" };
                    var barDiversitySet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<string>(), HoverBackgroundColor = new List<string>(), YAxisID = "y-diversity" };
                    barDataSet.Label = "Sim Id:" + t1.Item1 + " " + t1.Item2;
                    foreach (var machineName in data.Labels)
                    {

                        Kpi machine = null;
                        var t  = machines.Where(x => x.Name == machineName && x.SimulationConfigurationId == t1.Item1 && x.SimulationType == t1.Item2).Distinct();
                        machine = t.Single();

                        var percent = Math.Round(machine.Value * 100, 2);
                        // var wait = max - work;
                        barDataSet.Data.Add(percent);
                        barDataSet.BackgroundColor.Add(cc.Color[i].Substring(0, cc.Color[1].Length - 4) + "0.4)");
                        barDataSet.HoverBackgroundColor.Add(cc.Color[i].Substring(0, cc.Color[1].Length - 4) + "0.7)");

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
                    data.Datasets.Add(barDataSet);
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
