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

//von Malte: gesamte Klasse neu erzeugt, für Chart der Pheromone

namespace Master40.ViewComponents
{
    public partial class MachinePheromoneViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public MachinePheromoneViewComponent(ProductionDomainContext context)
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

                SimulationType simType = (paramsList[1].Equals("Decentral"))
                    ? SimulationType.Decentral
                    : SimulationType.Central;

                Chart chart = new Chart { Type = "scatter" };

                // charttype
                var cc = new ChartColor();
                // use available hight in Chart
                // use available hight in Chart
                var machinesKpi = _context.Kpis.Where(x => x.SimulationConfigurationId == Convert.ToInt32(paramsList[0])
                                                        && x.SimulationType == simType
                                                        && x.KpiType == KpiType.PheromoneHistory
                                                        && !x.IsKpi
                                                        && !x.IsFinal 
                                                        && x.SimulationNumber == Convert.ToInt32(paramsList[2]))
                                                     .OrderBy(g => g.Name).ToList();

                var settlingTime = _context.SimulationConfigurations.First(x => x.Id == Convert.ToInt32(paramsList[0])).SettlingStart;
                var machines = machinesKpi.Select(n => n.Name).Distinct().ToList();
                var data = new Data { Labels = machines };

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                foreach (var machine in machines)
                {
                    // add zero to start
                    var kpis = new List<LineScatterData> { new LineScatterData { x = "0", y = "1" } };
                    kpis.AddRange(machinesKpi.Where(x => x.Name == machine).OrderBy(x => x.Time)
                        .Select(x => new LineScatterData { x = x.Time.ToString(), y = (x.Value).ToString() }).ToList());

                    var lds = new LineScatterDataset()
                    {
                        Data = kpis,
                        BorderWidth = 2,
                        Label = machine,
                        ShowLine = true,
                        Fill = "false",
                        BackgroundColor = cc.Color[i],
                        BorderColor = cc.Color[i++],
                        LineTension = 0
                    };
                    data.Datasets.Add(lds);

                }

                data.Datasets.Add(new LineScatterDataset()
                {
                    Data = new List<LineScatterData> { new LineScatterData { x = "0", y = "100" }, new LineScatterData { x = Convert.ToDouble(settlingTime).ToString(), y = "100" } },
                    BorderWidth = 1,
                    Label = "Settling time",
                    BackgroundColor = "rgba(0, 0, 0, 0.1)",
                    BorderColor = "rgba(0, 0, 0, 0.3)",
                    ShowLine = true,
                    LineTension = 0,
                    PointRadius = new List<int> { 0, 0 }
                });

                chart.Data = data;

                // Specify xy Axis
                var xAxis = new List<Scale>() { new CartesianScale { Stacked = false, Display = true } };
                var yAxis = new List<Scale>()
                {
                    new CartesianScale()
                    {
                        Stacked = false, Ticks = new CartesianLinearTick {BeginAtZero = true, Min = 0, Max = 4}, Display = true,
                        Id = "first-y-axis", Type = "linear" , ScaleLabel = new ScaleLabel{ LabelString = "Value", Display = true, FontSize = 12 },
                    }
                };

                chart.Options = new Options()
                {
                    Scales = new Scales { XAxes = xAxis, YAxes = yAxis },
                    Responsive = true,
                    MaintainAspectRatio = true,
                    Legend = new Legend { Position = "bottom", Display = true, FullWidth = true },
                    Title = new Title { Text = "Machine Pheromone over Time", Position = "top", FontSize = 24, FontStyle = "bold", Display = true }
                };

                return chart;
            });

            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            return View($"MachinePheromone");
        }
    }
}
