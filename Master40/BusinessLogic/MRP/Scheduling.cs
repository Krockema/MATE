using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
