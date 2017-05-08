using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models;

namespace Master40.BusinessLogic.MRP
{

    interface IScheduling
    {
        void BackwardScheduling();
        void ForwardScheduling();
        void CapacityScheduling();
    }

    class Scheduling : IScheduling
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public Scheduling(MasterDBContext context)
        {
            Logger = new List<LogMessage>();
            _context = context;
        }
        void IScheduling.BackwardScheduling()
        {
            
        }

        void IScheduling.ForwardScheduling()
        {
            
        }

        void IScheduling.CapacityScheduling()
        {
            
        }
        
    }
}
