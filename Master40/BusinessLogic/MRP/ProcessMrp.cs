using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        void Process();
    
    }

    public class ProcessMrp : IProcessMrp
    {
        void IProcessMrp.Process()
        {
            //IDemandForecast demand = new DemandForecast();
            //demand.NetRequirement();
            //demand.GrossRequirement();
            IScheduling schedule = new Scheduling();
            schedule.BackwardScheduling();
            schedule.ForwardScheduling();
            schedule.CapacityScheduling();
            schedule.BatchSizeScheduling();
        }
    }

    
}