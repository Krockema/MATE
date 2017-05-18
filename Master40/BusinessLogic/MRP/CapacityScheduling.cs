using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{
    interface ICapacityScheduling
    {
        void GifflerThompsonScheduling();
    }

    class CapacityScheduling : ICapacityScheduling
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public CapacityScheduling(MasterDBContext context)
        {
            Logger = new List<LogMessage>();
            _context = context;
        }
        public void GifflerThompsonScheduling()
        {
            var demands = GetSchedule();

            
        }

        private List<IDemandToProvider> GetSchedule()
        {
            var demandRequests = new List<IDemandToProvider>();
            /*
            var demandRequestsFromOrderPart = _context.Demands.OfType<DemandOrderPart>().Include(a => a.OrderPart).Where(a => a.OrderPart.IsPlanned == false).ToList();
            foreach (var demandFromOrderPart in demandRequestsFromOrderPart)
            {
                demandRequests.Add(demandFromOrderPart);
            }
            var demandsFromStock = _context.Demands.OfType<DemandStock>();
            foreach (var demandFromStock in demandsFromStock)
            {
                demandRequests.Add(demandFromStock);

            }
            var demandProvider = new List<DemandProviderProductionOrder>();
            var productionOrderWorkSchedule = new List<ProductionOrderWorkSchedule>();
            foreach (var demand in demandRequests)
            {
                demandProvider = _context.Demands.OfType<DemandProviderProductionOrder>().Include(a => a.ProductionOrder).ThenInclude(a => a.ProductionOrderWorkSchedule).Where(a => a.DemandRequesterId == demand.DemandId).ToList();
                foreach (var demand in demandProvider)
                {
                    foreach (var schedule in demand.ProductionOrder.ProductionOrderWorkSchedule)
                    {
                        productionOrderWorkSchedule.Add(schedule);
                    }
                    
                }
               
            }*/
            return demandRequests;
        }
    }
    
}
