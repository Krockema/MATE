using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Master40.Data;
using Master40.Models.DB;
using Master40.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{
    public interface IProcessMrp
    {
        List<LogMessage> Logger { get; set; }
        Task Process(int orderId);
    }

    public class ProcessMrp : IProcessMrp
    {
        private readonly MasterDBContext _context;
        public List<LogMessage> Logger { get; set; }
        public ProcessMrp(MasterDBContext context)
        {
            System.Diagnostics.Debug.WriteLine("MRP service initialized");
            _context = context;

        }

        async Task IProcessMrp.Process(int orderId)
        {
            await Task.Run(() => {
            
                
                //execute demand forecast
                IDemandForecast demand = new DemandForecast(_context);
                //var productionOrders = demand.NetRequirement(demand.GrossRequirement(orderId));
                //Logger = demand.Logger;
            

                //execute scheduling
                //IScheduling schedule = new Scheduling(_context);
                //var manufacturingSchedule = schedule.CreateSchedule(orderId, productionOrders);
                //var backward = schedule.BackwardScheduling(manufacturingSchedule);
                //var forward = schedule.ForwardScheduling(manufacturingSchedule);
                //schedule.CapacityScheduling(backward, forward);
                //Logger = demand.Logger;

                var order = _context.OrderParts.Where(a => a.OrderId == orderId);
                foreach (var orderPart in order)
                {
                    
                    ExecutePlanning(null,orderPart.OrderPartId);
                }
                

            });
        }

        // gross, net requirement, create schedule, backward, forward, call children
        private void ExecutePlanning(DemandOrderPart demand,int orderPartId)
        {
            var orderPart = _context.OrderParts.Include(a => a.Article).Single(a => a.OrderPartId == orderPartId);
            if (demand == null)
            {
                demand = new DemandOrderPart()
                {
                    OrderPartId = orderPartId,
                    Quantity = orderPart.Quantity,
                    Article = orderPart.Article,
                    ArticleId = orderPart.ArticleId,
                    OrderPart = orderPart,
                    IsProvided = false,
                };
                
            }
           
            IDemandForecast demandForecast = new DemandForecast(_context);
            //IScheduling schedule = new Scheduling(_context);
            var productionOrders = demandForecast.NetRequirement(demand, orderPartId);
            foreach (var log in demandForecast.Logger)
            {
                Logger.Add(log);
            }
            
            //var manufacturingSchedule = schedule.CreateSchedule(orderId, productionOrders);
            //var backward = schedule.BackwardScheduling(manufacturingSchedule);
            //var forward = schedule.ForwardScheduling(manufacturingSchedule);
           
            var children =
                _context.ArticleBoms.Include(a => a.ArticleChild)
                    .Where(a => a.ArticleParentId == demand.ArticleId);
            if (children.Any())
            {
                foreach (var child in children)
                {
                    ExecutePlanning(new DemandOrderPart()
                    {
                        ArticleId = child.ArticleChildId,
                        Article = child.ArticleChild,
                        Quantity = productionOrders.Quantity * (int)child.Quantity
                        
                    }, orderPartId);
                }
            }

        }


    }


}