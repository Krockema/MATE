using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;
using Master40.Simulation.Simulation.SimulationData;
using Microsoft.EntityFrameworkCore;

namespace Master40.Simulation.Simulation
{
    public class PowsSimulationItem : ISimulationItem
    {
        public PowsSimulationItem(int productionOrderWorkScheduleId,int productionOrderId, int start, int end, ProductionDomainContext context)
        {
            SimulationState = SimulationState.Waiting;
            ProductionOrderWorkScheduleId = productionOrderWorkScheduleId;
            ProductionOrderId = productionOrderId;
            Start = start;
            End = end;
            NeedToAddNext = false;
            _context = context;
        }

        private ProductionDomainContext _context;
        public int Start { get; set; }
        public int End { get; set; }
        public int ProductionOrderWorkScheduleId { get; set; }
        public int ProductionOrderId { get; set; }
        public SimulationState SimulationState { get; set; }
        public bool NeedToAddNext { get; set; }
        
        public Task<bool> DoAtStart()
        {
            var pows = _context.ProductionOrderWorkSchedules.Single(a => a.Id == ProductionOrderWorkScheduleId);
            pows.ProducingState = ProducingState.Producing;
            _context.ProductionOrderWorkSchedules.Update(pows);
            _context.SaveChanges();
            var pobs = _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == ProductionOrderId);
            foreach (var pob in pobs)
            {
                _context.Stocks.Single(a => a.ArticleForeignKey == pob.DemandProductionOrderBoms.First().ArticleId).Current-= pob.Quantity;
            }
            return null;
        }
        
        public Task<bool> DoAtEnd<T>(List<TimeTable<T>.MachineStatus> listMachineStatus) where T : ISimulationItem
        {
            var pows = _context.ProductionOrderWorkSchedules.Single(a => a.Id == ProductionOrderWorkScheduleId);
            pows.ProducingState = ProducingState.Finished;
            _context.ProductionOrderWorkSchedules.Update(pows);
            _context.SaveChanges();
            listMachineStatus.Single(b => b.MachineId == _context.ProductionOrderWorkSchedules.Single(a => a.Id == ProductionOrderWorkScheduleId).MachineId).Free = true;
            var powslist = _context.ProductionOrderWorkSchedules.Include(a => a.ProductionOrder).Where(a => a.ProductionOrderId == ProductionOrderId);
            if (powslist.Single(a => a.Id == ProductionOrderWorkScheduleId).HierarchyNumber !=
                powslist.Max(a => a.HierarchyNumber)) return null;
            var articleId = _context.ProductionOrders.Single(a => a.Id == ProductionOrderId).ArticleId;
            _context.Stocks.Single(a => a.ArticleForeignKey == articleId).Current+= _context.SimulationConfigurations.Last().Lotsize;
            
            return null;
        }
    }
}
