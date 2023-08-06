using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Helpers;
using ChartJSCore.Models;
using Mate.DataCore.Data.Context;
using Mate.Extensions;
using Microsoft.AspNetCore.Mvc;
using Mate.Production.Core.Helper;

namespace Mate.ViewComponents
{
    public partial class MachineWorkloadViewComponent : ViewComponent
    {
        private readonly MateResultDb _context;

        public MachineWorkloadViewComponent(MateResultDb context)
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

                var endSum = _context.SimulationJobs.Where(predicate: x => x.CapabilityProvider == machine).Sum(x => x.End.ToFileTimeUtc());
                var startSum = _context.SimulationJobs.Where(predicate: x => x.CapabilityProvider == machine).Sum(x => x.Start.ToFileTimeUtc());
                var max = _context.SimulationJobs.Max(x => x.End.ToFileTimeUtc());
                var work = endSum - startSum;
                var wait = max - work;
                var cc = new ChartColors();
                data.Datasets.Add( item: new PieDataset{ Data = new List<double?>{ work, wait },
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
