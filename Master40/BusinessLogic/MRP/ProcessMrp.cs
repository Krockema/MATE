using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        
        void Process();
    
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly MasterDBContext _context;

        public ProcessMrp(MasterDBContext context)
        {
            _context = context;
            System.Diagnostics.Debug.WriteLine("MRP service initialized");
        }

        void IProcessMrp.Process()
        {
            IDemandForecast demand = new DemandForecast(_context);
            demand.NetRequirement();
            demand.GrossRequirement();
            IScheduling schedule = new Scheduling();
            schedule.BackwardScheduling();
            schedule.ForwardScheduling();
            schedule.CapacityScheduling();
            schedule.BatchSizeScheduling();

        }
    }

    
}