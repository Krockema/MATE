using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;
using Master40.DB.Enums;

namespace Master40.ViewComponents
{
    public partial class MachinesWorkLoadViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;
        private readonly ResultContext _resultContext;

        public MachinesWorkLoadViewComponent(ProductionDomainContext context, ResultContext resultContext)
        {
            _context = context;
            _resultContext = resultContext;
        }



        public async Task<IViewComponentResult> InvokeAsync(List<string> paramsList)
        {
            Task<Chart> generateChartTask; 
            if (Convert.ToInt32(value: paramsList[index: 3]) == 1)
            {
                generateChartTask = GenerateChartTaskOverTime(paramsList: paramsList);
            }
            else  generateChartTask = GenerateChartTask(paramsList: paramsList);

            // create JS to Render Chart.
            ViewData[index: "chart"] = await generateChartTask;
            ViewData[index: "Type"] = paramsList[index: 1];
            ViewData[index: "OverTime"] = paramsList[index: 3];
            return View(viewName: $"MachinesWorkLoad");
        }

        private Task<Chart> GenerateChartTask(List<string> paramsList)
        {
            var generateChartTask = Task.Run(function: () =>
            {
                if (!_resultContext.SimulationOperations.Any())
                {
                    return null;
                }

                SimulationType simType = (paramsList[index: 1].Equals(value: "Decentral"))
                    ? SimulationType.Decentral
                    : SimulationType.Central;

                Chart chart = new Chart
                {
                    Type = Enums.ChartType.Bar                         
                };

                // charttype

                // use available hight in Chart
                // use available hight in Chart
                var machines = _resultContext.Kpis.Where(predicate: x => x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                                        && x.SimulationType == simType
                                                        && x.KpiType == KpiType.MachineUtilization
                                                        && x.IsKpi
                                                        && x.IsFinal && x.SimulationNumber == Convert.ToInt32(paramsList[2]))
                                           .OrderByDescending(keySelector: g => g.Name)
                    .ToList();
                var data = new Data {Labels = machines.Select(selector: n => n.Name).ToList()};

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                var cc = new ChartColors();
                
                //var max = _context.SimulationWorkschedules.Max(x => x.End) - 1440; 
                var barDataSet = new BarDataset {Data = new List<double>(), BackgroundColor = new List<ChartColor>(), HoverBackgroundColor = new List<ChartColor>(), YAxisID ="y-normal"};
                var barDiversityInvisSet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<ChartColor>(), HoverBackgroundColor = new List<ChartColor>(), YAxisID= "y-diversity"};
                var barDiversitySet = new BarDataset { Data = new List<double>(), BackgroundColor = new List<ChartColor>(), HoverBackgroundColor = new List<ChartColor>(), YAxisID ="y-diversity"};
                foreach (var machine in machines)
                {
                    var percent = Math.Round(value: machine.Value * 100, digits: 2);
                    // var wait = max - work;
                    barDataSet.Data.Add(item: percent);
                    barDataSet.BackgroundColor.Add(item: cc.Get(i, 0.4));
                    barDataSet.HoverBackgroundColor.Add(item: cc.Get(i, 0.7));

                    var varianz = machine.Count * 100;

                    barDiversityInvisSet.Data.Add(item: percent - Math.Round(value: varianz / 2, digits: 2));
                    barDiversityInvisSet.BackgroundColor.Add(item: ChartColors.Transparent);
                    barDiversityInvisSet.HoverBackgroundColor.Add(item: ChartColors.Transparent);

                    barDiversitySet.Data.Add(item: Math.Round(value: varianz, digits: 2));
                    barDiversitySet.BackgroundColor.Add(item: cc.Get(i, 0.8));
                    barDiversitySet.HoverBackgroundColor.Add(item: cc.Get(i, 1));
                    i++;
                }

                data.Datasets.Add(item: barDataSet);
                data.Datasets.Add(item: barDiversityInvisSet);
                data.Datasets.Add(item: barDiversitySet);
                
                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() { new CartesianScale { Stacked = true, Id = "x-normal", Display = true } };
                var yAxis = new List<Scale>() 
                {
                    new CartesianScale { Stacked = true, Display = true, Ticks = new CartesianLinearTick { BeginAtZero = true, Min = 0, Max = 100}, Id = "y-normal" },
                    new CartesianScale {
                        Stacked = true, Ticks = new CartesianLinearTick {BeginAtZero = true, Min = 0, Max = 100}, Display = false,
                        Id = "y-diversity", ScaleLabel = new ScaleLabel{ LabelString = "Value in %", Display = false, FontSize = 12 },
                    },
                };
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options()
                {
                    Scales = new Scales { XAxes = xAxis, YAxes = yAxis },
                    MaintainAspectRatio = false,
                    Responsive = true,
                    Title = new Title { Text = "Machine Workloads", Position = "top", FontSize = 24, FontStyle = "bold", Display = true },
                    Legend = new Legend { Position = "bottom", Display = false }
                };

                return chart;
            });
            return generateChartTask;
        }

        private Task<Chart> GenerateChartTaskOverTime(List<string> paramsList)
        {
            var generateChartTask = Task.Run(function: () =>
            {
                if (!_resultContext.SimulationOperations.Any())
                {
                    return null;
                }

                SimulationType simType = (paramsList[index: 1].Equals(value: "Decentral"))
                    ? SimulationType.Decentral
                    : SimulationType.Central;

                Chart chart = new Chart { Type = Enums.ChartType.Scatter };

                // charttype
                var cc = new ChartColors();
                // use available hight in Chart
                // use available hight in Chart
                var machinesKpi = _resultContext.Kpis.Where(predicate: x => x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                                        && x.SimulationType == simType
                                                        && x.KpiType == KpiType.MachineUtilization
                                                        && !x.IsKpi
                                                        && !x.IsFinal && x.SimulationNumber == Convert.ToInt32(paramsList[2]))
                    .ToList();
                var settlingTime = _resultContext.SimulationConfigurations.First(predicate: x => x.Id == Convert.ToInt32(paramsList[0])).SettlingStart;
                var machines = machinesKpi.Select(selector: n => n.Name).Distinct().ToList();
                var data = new Data { Labels = machines };

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                foreach (var machine in machines)
                {
                    // add zero to start
                    var kpis = new List<LineScatterData> { new LineScatterData {  X = "0", Y = "0" } };
                    kpis.AddRange(collection: machinesKpi.Where(predicate: x => x.Name == machine).OrderBy(keySelector: x => x.Time)
                        .Select(selector: x => new LineScatterData { X = x.Time.ToString() , Y = (x.Value * 100).ToString() }).ToList());

                    var lds = new LineScatterDataset()
                    {
                        Data = kpis,
                        BorderWidth = 2,
                        Label = machine,
                        ShowLine = true,
                        Fill = "false",
                        BackgroundColor = cc.Get(i),
                        BorderColor = cc.Get(i++),
                        LineTension = 0
                    };
                    data.Datasets.Add(item: lds);

                }

                
                data.Datasets.Add(item: new LineScatterDataset()
                {
                    Data = new List<LineScatterData> { new LineScatterData { X = "0", Y = "100" }, new LineScatterData { X = Convert.ToDouble(value: settlingTime).ToString(), Y = "100" } },
                    BorderWidth = 1,
                    Label = "Settling time",
                    BackgroundColor = ChartJSCore.Helpers.ChartColor.FromRgba(0, 0, 0, 0.1),
                    BorderColor = ChartJSCore.Helpers.ChartColor.FromRgba(0, 0, 0, 0.3),
                    ShowLine = true,
                    //Fill = true,
                    //SteppedLine = false,
                    LineTension = 0,
                    PointRadius = new List<int> { 0, 0}
                });

                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() { new CartesianScale { Stacked = false, Display = true } };
                var yAxis = new List<Scale>()
                {
                    new CartesianScale()
                    {
                        Stacked = false, Ticks = new CartesianLinearTick {BeginAtZero = true, Min = 0, Max = 100}, Display = true,
                        Id = "first-y-axis", Type = "linear" , ScaleLabel = new ScaleLabel{ LabelString = "Value in %", Display = true, FontSize = 12 },
                    }
                };
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options()
                {
                    Scales = new Scales { XAxes = xAxis, YAxes = yAxis },
                    Responsive = true,
                    MaintainAspectRatio = true,
                    Legend = new Legend { Position = "bottom", Display = true, FullWidth = true },
                    Title = new Title { Text = "Machine Workload over Time", Position = "top", FontSize = 24, FontStyle = "bold", Display = true }
                };

                return chart;
            });
            return generateChartTask;
        }
    }
}
