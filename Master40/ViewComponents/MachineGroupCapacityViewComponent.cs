using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Repository;
using Microsoft.AspNetCore.Mvc;
using ChartJSCore.Models;
using ChartJSCore.Models.Bar;
using Master40.DB.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Master40.Extensions;

namespace Master40.ViewComponents
{
    public partial class MachineGroupCapacityViewComponent : ViewComponent
    {
        private readonly ProductionDomainContext _context;

        public MachineGroupCapacityViewComponent(ProductionDomainContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var generateCharTask = Task.Run(() =>
            {
                int schedulingState = 1;
                if (Request.Method == "POST")
                {
                    // catch scheduling state
                    schedulingState = Convert.ToInt32(Request.Form["SchedulingState"]);
                }

                Chart chart = new Chart();

                chart.Type = "bar";

                Data data = new Data();
                data.Labels = GetRangeForSchedulingType(schedulingState);
                var machineGroups = _context.MachineGroups.Select(x => x.Id);


                data.Datasets = new List<Dataset>();
                if (data.Labels.Any())
                {
                    foreach (var id in machineGroups)
                    {
                        data.Datasets.Add(GetCapacityForMachineGroupById(id, Convert.ToInt32(data.Labels.First()),
                            Convert.ToInt32(data.Labels.Last()), schedulingState));
                    }
                }
                
                chart.Data = data;

                var axis = new List<Scale>() {new BarScale {Stacked = false}};
                chart.Options = new Options() {Scales = new Scales {XAxes = axis, YAxes = axis}};

                return chart;
            });
           
            ViewData["chart"] = await generateCharTask;

            return View($"MachineGroupCapacity");

        }

        private List<string> GetRangeForSchedulingType(int schedulingState)
        {
            List<string> labeList = new List<string>();
            int min, max;

            switch (schedulingState)
            {
                case 1:
                    min = _context.ProductionOrderWorkSchedule.Where(x => x.StartForward < 9000).Min(x => x.StartForward);
                    max = _context.ProductionOrderWorkSchedule.Where(x => x.EndForward < 9000).Max(x => x.EndForward);
                    break;
                case 2:
                    min = _context.ProductionOrderWorkSchedule.Where(x => x.StartBackward < 9000).Min(x => x.StartBackward);
                    max = _context.ProductionOrderWorkSchedule.Where(x => x.EndBackward < 9000).Max(x => x.EndBackward);
                    break;
                default:
                    min = _context.ProductionOrderWorkSchedule.Where(x => x.Start < 9000).Min(x => x.Start);
                    max = _context.ProductionOrderWorkSchedule.Where(x => x.End < 9000).Max(x => x.End);
                    break;
            }

            for (int i = min; i < max; i++)
            {
                labeList.Add(i.ToString());
            }
            return labeList;
        }



        private BarDataset GetCapacityForMachineGroupById(int machineGroupId, int minRange, int maxRange, int schedulingState)
        {
            /*
            select ts.Time, Count(ts.Id)
            from[ProductionOrderWorkSchedulesByTimeSteps] ts
                join MachineGroupProductionOrderWorkSchedules ws on ts.MachineGroupProductionOrderWorkScheduleId = ws.Id
            where Time< 9000 AND MachineGroupId = 1
            group by Time, MachineGroupId
            Order By Time
            */
            /*
            var productionOrderWorkSchedulesBy =
                _context.ProductionOrderWorkSchedulesByTimeSteps
                    .Include(x => x.MachineGroupProductionOrderWorkSchedule)
                    .Where(x => x.MachineGroupProductionOrderWorkSchedule.MachineGroupId == machineGroupId && x.Time < 9000)
                    .GroupBy(x => x.Time).Select(n => new {
                                                            MetricName = n.Key,
                                                            MetricCount = n.Count()
                                                        }).ToList();
            */

            var productionOrderWorkSchedulesBy = _context.ProductionOrderWorkSchedule.Where(x => x.MachineGroupId == machineGroupId).AsNoTracking();
            
            var data = new List<double>();
            var step = productionOrderWorkSchedulesBy.Count();
            for (var i = minRange; i < maxRange - 1 ; i++)
            {
                int item;
                switch (schedulingState)
                {
                    case 1:
                        item = productionOrderWorkSchedulesBy.Count(x => x.StartForward <= i && x.EndForward >= i);
                        break;
                    case 2:
                        item = productionOrderWorkSchedulesBy.Count(x => x.StartBackward <= i && x.EndBackward >= i);
                        break;
                    default:
                        item = productionOrderWorkSchedulesBy.Count(x => x.Start <= i && x.End >= i);
                        break;
                }
                data.Add(item);
            }

            var dataset = new BarDataset()
            {
                Label = "MachineGroup " + machineGroupId.ToString(),
                BackgroundColor = new List<string> { new ChartColor().Color[machineGroupId - 1] },
                Data = data,
            };
            
            return dataset;


            
        }
    }
}
