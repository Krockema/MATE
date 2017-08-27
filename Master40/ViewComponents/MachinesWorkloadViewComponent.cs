using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;
using ChartJSCore.Models.Bar;

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

               Chart chart = new Chart();

                // charttype
                chart.Type = "bar";

                // use available hight in Chart
                // use available hight in Chart
                chart.Options = new Options { MaintainAspectRatio = true };
                var machines = _context.SimulationWorkschedules.Where(x => x.SimulationId == Convert.ToInt32(paramsList[0]) && x.SimulationType == paramsList[1])
                    .Select(x => x.Machine)
                    .Distinct()
                    .ToList();
                var data = new Data { Labels = machines };
                
                //var yMaxScale = 0;

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var i = 0;
                var max = _context.SimulationWorkschedules.Max(x => x.End) - 1440; 
                var barDataSet = new BarDataset{ Data = new List<double>(), BackgroundColor = new List<string>()};

                foreach (var machine in machines)
                {

                    barDataSet.Label = "Machines";
                    var machineWork =
                        _context.SimulationWorkschedules.Where(
                            x => x.Machine == machine && x.SimulationId == Convert.ToInt32(paramsList[0]) && x.SimulationType == paramsList[1]).ToList();

                    var endSum = machineWork.Sum(x => x.End);
                    var startSum = machineWork.Sum(x => x.Start);
                    var work = endSum - startSum;
                    var percent = Math.Round((double)work / max * 100, 2);
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
                chart.Options = new Options() { Scales = new Scales { XAxes = xAxis, YAxes = yAxis }, MaintainAspectRatio = false, Responsive = true };

                return chart;
            });

            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["Type"] = paramsList[1];
            return View($"MachinesWorkLoad");
        }
    }
}
