using Master40.BusinessLogicCentral.MRP;
using Master40.DB.Data.Context;
using Master40.MessageSystem.SignalR;

namespace Master40.Simulation.Simulation
{
    public class ProcessMrpSim : ProcessMrp , IProcessMrp
    {
        public ProcessMrpSim(ProductionDomainContext ctx, IMessageHub msgHub) : 
            base(ctx, new Scheduling(ctx), new CapacityScheduling(ctx), msgHub, new RebuildNets(ctx) )
        {
            // copy aditional Tables from Realcontext (needs to be Injected as well) 

        }
    }
}
