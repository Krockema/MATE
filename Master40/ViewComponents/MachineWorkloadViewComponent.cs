﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public partial class MachineWorkloadViewComponent : ViewComponent
    {
        private readonly ResultContext _context;

        public MachineWorkloadViewComponent(ResultContext context)
        {
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(string machine)
        {
            var generateChartTask = Task.Run(function: () =>
            {

                if (!_context.SimulationJobs.Any())
                {
                    return null;
                }

               Chart chart = new Chart();

                // charttype
                chart.Type = Enums.ChartType.Pie;

                // use available hight in Chart
                chart.Options = new Options { MaintainAspectRatio = true};
                var data = new Data();

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var endSum = _context.SimulationJobs.Where(predicate: x => x.Resource == machine).Sum(selector: x => x.End);
                var startSum = _context.SimulationJobs.Where(predicate: x => x.Resource == machine).Sum(selector: x => x.Start);
                var max = _context.SimulationJobs.Max(selector: x => x.End);
                var work = endSum - startSum;
                var wait = max - work;
                var cc = new ChartColors();
                data.Datasets.Add( item: new PieDataset{ Data = new List<double>{ work, wait },
                    BackgroundColor = new List<ChartColor> { cc.Get(2), cc.Get(0) } } );

                data.Labels = new string[] {"Work " + Math.Round(d: Convert.ToDecimal(value: work) / max*100, decimals: 2) + " %",
                                            "Wait " + Math.Round(d: Convert.ToDecimal(value: wait) / max*100, decimals: 2) + " %"};

                chart.Data = data;
                chart.Options = new Options() { MaintainAspectRatio = false, Responsive = true };

                return chart;
            });
           
            // create JS to Render Chart.
            ViewData[index: "chart"] = await generateChartTask;
            ViewData[index: "machine"] = machine;

            return View(viewName: $"MachineWorkload");

        }
    }
}
