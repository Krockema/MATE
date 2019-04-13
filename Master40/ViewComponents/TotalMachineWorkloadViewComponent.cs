using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using Master40.DB.Data.Context;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public partial class TotalMachineWorkloadViewComponent : ViewComponent
    {
        private readonly ResultContext _context;

        public TotalMachineWorkloadViewComponent(ResultContext context)
        {
            _context = context;
        }



        public async Task<IViewComponentResult> InvokeAsync(string machine)
        {
            var generateChartTask = Task.Run(() =>
            {

                if (!_context.SimulationOperations.Any())
                {
                    return null;
                }

               Chart chart = new Chart();

                // charttype
                chart.Type = "pie";

                // use available hight in Chart
                chart.Options = new Options { MaintainAspectRatio = true};
                var data = new Data();

                // create Dataset for each Lable
                data.Datasets = new List<Dataset>();

                var endSum = _context.SimulationOperations.Where(x => x.Machine == machine).Sum(x => x.End);
                var startSum = _context.SimulationOperations.Where(x => x.Machine == machine).Sum(x => x.Start);
                var max = _context.SimulationOperations.Max(x => x.End);
                var work = endSum - startSum;
                var wait = max - work;
                data.Datasets.Add( new PieDataset{ Data = new List<double>{ work, wait },
                    BackgroundColor = new List<string> { new ChartColor().Color[2], new ChartColor().Color[0] } } );

                data.Labels = new string[] {"Work " + Math.Round(Convert.ToDecimal(work) / max*100, 2) + " %",
                                            "Wait " + Math.Round(Convert.ToDecimal(wait) / max*100, 2) + " %"};

                chart.Data = data;
                chart.Options = new Options() { MaintainAspectRatio = false, Responsive = true };

                return chart;
            });
           
            // create JS to Render Chart.
            ViewData["chart"] = await generateChartTask;
            ViewData["machine"] = machine;

            return View($"MachineWorkload");

        }
    }
}
