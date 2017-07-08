using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Master40.Simulation.Simulation.SimulationData;
using Microsoft.EntityFrameworkCore;

namespace Master40.Simulation.Simulation
{
    public class PowsSimulationItem : ISimulationItem
    {
        public PowsSimulationItem(int productionOrderWorkScheduleId,int productionOrderId, int start, int end, MasterDBContext context)
        {
            SimulationState = SimulationState.Waiting;
            ProductionOrderWorkScheduleId = productionOrderWorkScheduleId;
            ProductionOrderId = productionOrderId;
            Start = start;
            End = end;
            NeedToAddNext = false;
            _context = context;
        }

        private MasterDBContext _context;
        public int Start { get; set; }
        public int End { get; set; }
        public int ProductionOrderWorkScheduleId { get; set; }
        public int ProductionOrderId { get; set; }
        public SimulationState SimulationState { get; set; }
        public bool NeedToAddNext { get; set; }
        
        public Task<bool> DoAtStart()
        {
            var pobs = _context.ProductionOrderBoms.Include(a => a.ProductionOrderChild).Where(a => a.ProductionOrderParentId == ProductionOrderId);
            foreach (var pob in pobs)
            {
                if (!_context.ProductionOrderBoms.Any(a => a.ProductionOrderParentId == pob.ProductionOrderChildId))
                    _context.Stocks.Single(a => a.ArticleForeignKey == pob.ProductionOrderChild.ArticleId).Current-= pob.Quantity;
            }
            return null;
        }
        
        public Task<bool> DoAtEnd<T>(List<TimeTable<T>.MachineStatus> listMachineStatus) where T : ISimulationItem
        {
            listMachineStatus.Single(b => b.MachineId == _context.ProductionOrderWorkSchedule.Single(a => a.Id == ProductionOrderWorkScheduleId).MachineId).Free = true;
            var pows = _context.ProductionOrderWorkSchedule.Include(a => a.ProductionOrder).Where(a => a.ProductionOrderId == ProductionOrderId);
            if (pows.Single(a => a.Id == ProductionOrderWorkScheduleId).HierarchyNumber !=
                pows.Max(a => a.HierarchyNumber)) return null;
            var articleId = _context.ProductionOrders.Single(a => a.Id == ProductionOrderId).ArticleId;
            _context.Stocks.Single(a => a.ArticleForeignKey == articleId).Current++;

            return null;
        }
    }
}
