using System;
using System.Collections.Generic;
using System.Text;
using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.MessageSystem.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Master40.Simulation.Simulation
{
    public class ProcessMrpSim : ProcessMrp , IProcessMrp
    {
        public ProcessMrpSim(MasterDBContext ctx, IMessageHub msgHub) : 
            base(ctx, new Scheduling(ctx), new CapacityScheduling(ctx), msgHub, new RebuildNets(ctx) )
        {
            // copy aditional Tables from Realcontext (needs to be Injected as well) 

        }
    }
}
