using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models.DB;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        
        void Process(int orderId);
    
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly IDemandForecast _demandForecast;

        public ProcessMrp(IDemandForecast demandForecast)
        {
            _demandForecast = demandForecast;
            System.Diagnostics.Debug.WriteLine("MRP service initialized");
            
        }

        void IProcessMrp.Process(int orderId)
        {
            _demandForecast.GrossRequirement(
                _demandForecast.NetRequirement(orderId));

            IScheduling schedule = new Scheduling();
            schedule.BackwardScheduling();
            schedule.ForwardScheduling();
            schedule.CapacityScheduling();
            schedule.BatchSizeScheduling();
        }
    }

    
}