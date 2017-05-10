using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models.DB;
using Master40.Models;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        List<LogMessage> Logger { get; set; }
        Task Process(int orderId);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public ProcessMrp(MasterDBContext context)
        {
            System.Diagnostics.Debug.WriteLine("MRP service initialized");
            _context = context;

        }

        async Task IProcessMrp.Process(int orderId)
        {
            await Task.Run(() => {
            
                //execute demand forecast
                //IDemandForecast demand = new DemandForecast(_context);
                //var productionOrders = demand.NetRequirement(demand.GrossRequirement(orderId));
                //Logger = demand.Logger;
            

                //execute scheduling
                IScheduling schedule = new Scheduling(_context);
                //var manufacturingSchedule = schedule.CreateSchedule(orderId, productionOrders);
                //var backward = schedule.BackwardScheduling(manufacturingSchedule);
                //var forward = schedule.ForwardScheduling(manufacturingSchedule);
                //schedule.CapacityScheduling(backward, forward);
                //Logger = demand.Logger;
                
            });
        }
    }


}