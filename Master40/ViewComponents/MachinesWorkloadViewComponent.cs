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
            var generateChartTask = Task.Run(() =>
            {

                if (!_context.SimulationWorkschedules.Any())
                {
                    return null;
                }

                SimulationType simType = (paramsList[1].Equals("Decentral")) ? SimulationType.Decentral : SimulationType.Central;
                
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
                                                    && x.KpiType == KpiType.MachineUtilization) 
                                                    .ToList();
                var data = new Data { Labels = machines.Select(n => n.Name).ToList() };

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                var max = _context.SimulationWorkschedules.Max(x => x.End) - 1440; 
                var barDataSet = new BarDataset{ Data = new List<double>(), BackgroundColor = new List<string>()};

                foreach (var machine in machines)
                {
                    var percent = Math.Round(machine.Value / max * 100, 2);
                    // var wait = max - work;
                    barDataSet.Data.Add(percent);
                    barDataSet.BackgroundColor.Add(new ChartColor().Color[i]);
                    i++;
                }

                data.Datasets.Add(barDataSet);
                chart.Data = data;

                // Specifie xy Axis
                var xAxis = new List<Scale>() { new BarScale { Stacked = false } };
                var yAxis = new List<Scale>() { new BarScale { Stacked = false, Ticks = new Tick { BeginAtZero = true, Min = 0, Max = 100 } } };
                //var yAxis = new List<Scale>() { new BarScale{ Ticks = new CategoryTick { Min = "0", Max  = (yMaxScale * 1.1).ToString() } } };
                chart.Options = new Options() { Scales = new Scales { XAxes = xAxis, YAxes = yAxis }, MaintainAspectRatio = false, Responsive = true, Legend = new Legend { Display = false } };

                return chart;
            });

            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            return View($"MachinesWorkLoad");
        }
    }
}
