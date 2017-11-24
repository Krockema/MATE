using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;
using ChartJSCore.Models.Bar;
using Master40.DB.Enums;

namespace Master40.ViewComponents
{
    public partial class MachinesWorkLoadViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public MachinesWorkLoadViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            Task<Chart> generateChartTask; 
            if (Convert.ToInt32(paramsList[3]) == 1)
            {
                generateChartTask = GenerateChartTaskOverTime(paramsList);
            }
            else  generateChartTask = GenerateChartTask(paramsList);

            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            ViewData["OverTime"] = paramsList[3];
            return View($"MachinesWorkLoad");
        }

        private Task<Chart> GenerateChartTask(List<string> paramsList)
        {
            var generateChartTask = Task.Run(() =>
            {
                if (!_context.SimulationWorkschedules.Any())
                {
                    return null;
                }

                SimulationType simType = (paramsList[1].Equals("Decentral"))
                    ? SimulationType.Decentral
                    : SimulationType.Central;

                Chart chart = new Chart
                {
                    Type = "bar",
                    Options = new Options {MaintainAspectRatio = true}
                };

                // charttype

                // use available hight in Chart
                // use available hight in Chart
                var machines = _context.Kpis.Where(x => x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                                        && x.SimulationType == simType
                                                        && x.KpiType == KpiType.MachineUtilization
                                                        && x.IsKpi
                                                        && x.IsFinal && x.SimulationNumber == Convert.ToInt32(paramsList[2]))
                                           .OrderByDescending(g => g.Name)
                    .ToList();
                var data = new Data {Labels = machines.Select(n => n.Name).ToList()};

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                //var max = _context.SimulationWorkschedules.Max(x => x.End) - 1440; 
                var barDataSet = new BarDataset {Data = new List<double>(), BackgroundColor = new List<string>()};

                foreach (var machine in machines)
                {
                    var percent = Math.Round(machine.Value * 100, 2);
                    // var wait = max - work;
                    barDataSet.Data.Add(percent);
                    barDataSet.BackgroundColor.Add(new ChartColor().Color[i]);
                    i++;
                }

                data.Datasets.Add(barDataSet);
                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() {new BarScale {Stacked = false}};
                var yAxis = new List<Scale>() 
                {
                    new BarScale
                    {
                        Stacked = false, Ticks = new Tick {BeginAtZero = true, Min = 0, Max = 100},
                        Id = "first-y-axis", Type = "linear" , ScaleLabel = new ScaleLabel{ LabelString = "Value in %", Display = true, FontSize = 12 },
                    }
                };
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options()
                {
                    Scales = new Scales {XAxes = xAxis, YAxes = yAxis},
                    MaintainAspectRatio = false,
                    Responsive = true,
                    Legend = new Legend {Display = false}
                };

                return chart;
            });
            return generateChartTask;
        }

        private Task<Chart> GenerateChartTaskOverTime(List<string> paramsList)
        {
            var generateChartTask = Task.Run(() =>
            {
                if (!_context.SimulationWorkschedules.Any())
                {
                    return null;
                }

                SimulationType simType = (paramsList[1].Equals("Decentral"))
                    ? SimulationType.Decentral
                    : SimulationType.Central;

                Chart chart = new Chart
                {
                    Type = "scatter",
                    Options = new Options
                    {
                        MaintainAspectRatio = true,
                        Legend = new Legend { Position = "bottom", Display = true, FullWidth = true },
                        Title = new Title { Text = "Machine Workload over Time", Position = "top", FontSize = 24, FontStyle = "bold" }
                    }
                };

                // charttype
                var cc = new ChartColor();
                // use available hight in Chart
                // use available hight in Chart
                var machinesKpi = _context.Kpis.Where(x => x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                                        && x.SimulationType == simType
                                                        && x.KpiType == KpiType.MachineUtilization
                                                        && !x.IsKpi
                                                        && !x.IsFinal && x.SimulationNumber == Convert.ToInt32(paramsList[2]))
                    .ToList();
                var machines = machinesKpi.Select(n => n.Name).Distinct().ToList();
                var data = new Data { Labels = machines };

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                foreach (var machine in machines)
                {
                    // add zero to start
                    var kpis = new List<LineScatterData> { new LineScatterData { x = 0, y = 0 } };
                    kpis.AddRange(machinesKpi.Where(x => x.Name == machine).OrderBy(x => x.Time)
                        .Select(x => new LineScatterData { x = x.Time, y = x.Value * 100 }).ToList());

                    var lds = new LineScatterDataset()
                    {
                        Data = kpis,
                        BorderWidth = 2,
                        Label = machine,
                        ShowLine = true,
                        SteppedLine = false,
                        BackgroundColor = cc.Color[i],
                        BorderColor = cc.Color[i++],
                        Fill = false,
                        //LineTension = 0
                    };
                    data.Datasets.Add(lds);

                }
                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() { new RadialLinearScale { Stacked = false } };
                var yAxis = new List<Scale>()
                {
                    new RadialLinearScale()
                    {
                        Stacked = false, Ticks = new Tick {BeginAtZero = true, Min = 0, Max = 100},
                        Id = "first-y-axis", Type = "linear" , ScaleLabel = new ScaleLabel{ LabelString = "Value in %", Display = true, FontSize = 12 },
                    }
                };
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options()
                {
                    Scales = new Scales { XAxes = xAxis, YAxes = yAxis },
                    MaintainAspectRatio = false,
                    Responsive = true,
                    Legend = new Legend { Display = true, Position = "bottom"}
                };

                return chart;
            });
            return generateChartTask;
        }
    }
}
