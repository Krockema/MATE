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
        void Process(int orderId);
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

        void IProcessMrp.Process(int orderId)
        {
            //execute demand forecast
            IDemandForecast demand = new DemandForecast(_context);
            demand.NetRequirement(demand.GrossRequirement(orderId));
            Logger = demand.Logger;
            

            //execute scheduling
            IScheduling schedule = new Scheduling();
            schedule.BackwardScheduling();
            schedule.ForwardScheduling();
            schedule.CapacityScheduling();
            
        }
    }


}