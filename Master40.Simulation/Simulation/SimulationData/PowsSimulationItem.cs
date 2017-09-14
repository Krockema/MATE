﻿using System;
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
            if (HasHierarchyChildren(pows))
            {
                _context.SaveChanges();
                return null;
            }

            //set bom to be finished
            var boms = _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == ProductionOrderId);
            foreach (var bom in boms)
            {
                bom.State = State.Finished;
                var requester = bom.DemandProductionOrderBoms;
                
                foreach (var req in requester.Where(a => a.State != State.Finished))
                {
                    //find DemandProviderStock to set them ready
                    foreach (var provider in req.DemandProvider.OfType<DemandProviderStock>())
                    {
                            provider.State = State.Finished;
                    }
                    req.State = State.Finished;
                }
                
            }
            _context.UpdateRange(boms);
            var pobs = _context.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == ProductionOrderId);
            foreach (var pob in pobs)
            {
                foreach (var dpob in pob.DemandProductionOrderBoms)
                {
                    var stock = _context.Stocks.Single(a =>
                        a.ArticleForeignKey == dpob.ArticleId);
                    stock.Current -= pob.Quantity;
                    _context.StockExchanges.Add(new StockExchange()
                    {
                        ExchangeType = ExchangeType.Withdrawal,
                        Quantity = pob.Quantity,
                        StockId = stock.Id,
                        RequiredOnTime = _context.ProductionOrders.Single(a => a.Id == ProductionOrderId).Duetime
                    });
                    _context.Stocks.Update(stock);
                    _context.SaveChanges();
                }
            }
            _context.SaveChanges();


            return null;
        }

        private bool HasHierarchyChildren(ProductionOrderWorkSchedule pows)
        {
            var powslist = _context.ProductionOrderWorkSchedules.Where(a => a.ProductionOrderId == pows.ProductionOrderId);
            return pows.HierarchyNumber != powslist.Min(a => a.HierarchyNumber);
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
            var stock = _context.Stocks.Include(x => x.StockExchanges).Single(a => a.ArticleForeignKey == articleId);
            var quantity = _context.SimulationConfigurations.Last().Lotsize;
            stock.Current += quantity;
            stock.StockExchanges.Add(new StockExchange()
            {
                ExchangeType = ExchangeType.Insert,
                Quantity = quantity,
                StockId = stock.Id,
                RequiredOnTime = _context.ProductionOrders.Single(a => a.Id == ProductionOrderId).Duetime
            });
            _context.Update(stock);
            _context.SaveChanges();
            return null;
        }
    }
}