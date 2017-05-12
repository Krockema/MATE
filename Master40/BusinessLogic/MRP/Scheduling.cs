using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogic.Helper;
using Master40.Data;
using Master40.Models;
using Master40.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Master40.BusinessLogic.MRP
{

    interface IScheduling
    {
        void CreateSchedule(int orderPartId, ProductionOrder productionOrder);
        void BackwardScheduling(ProductionOrder productionOrder);
        void ForwardScheduling(ProductionOrder productionOrder);
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

        public void CreateSchedule(int orderPartId, ProductionOrder productionOrder)
        {
            var headOrder = _context.OrderParts.Include(a => a.Order).Single(a => a.OrderPartId == orderPartId);
            
            var timeHelper = headOrder.Order.DueTime;

            //get abstract workSchedule
            var abstractWorkSchedules = _context.WorkSchedules.Where(a => a.ArticleId == productionOrder.ArticleId);

            foreach (var abstractWorkSchedule in abstractWorkSchedules)
            {


                //add specific workSchedule
                var workSchedule = new ProductionOrderWorkSchedule()
                {
                    Duration = abstractWorkSchedule.Duration * (int) productionOrder.Quantity,
                    HierarchyNumber = abstractWorkSchedule.HierarchyNumber,
                    MachineGroupId = abstractWorkSchedule.MachineGroupId,
                    MachineGroup = abstractWorkSchedule.MachineGroup,
                    MachineTool = abstractWorkSchedule.MachineTool,
                    MachineToolId = abstractWorkSchedule.MachineToolId,
                    Name = abstractWorkSchedule.Name,
                    End = -1,
                    Start = timeHelper,
                    ProductionOrderId = productionOrder.ProductionOrderId
                };
                _context.ProductionOrderWorkSchedule.Add(workSchedule);

            }

        }
        void IScheduling.BackwardScheduling(ProductionOrder productionOrder)
        {
           var workSchedules = _context.ProductionOrderWorkSchedule
                .Include(a => a.ProductionOrder)
                .ThenInclude(a => a.ProductionOrderBoms)
                .ThenInclude(a => a.ProductionOrderParent)
                .Where(a => a.ProductionOrderId == productionOrder.ProductionOrderId);
            foreach (var workSchedule in workSchedules)
            {
                ProductionOrderBom parent = null;
                foreach (var pob in workSchedule.ProductionOrder.ProductionOrderBoms)
                {
                    if (pob.ProductionOrderParentId != workSchedule.ProductionOrder.ArticleId)
                        parent = pob;
                }
                //set start- and endtime 
                if (parent != null)
                {
                    workSchedule.End = parent.Start;
                }
                else
                {
                    //initial value of start is duetime of the order
                    workSchedule.End = workSchedule.Start;
                }
                workSchedule.Start = workSchedule.End - workSchedule.Duration;
                _context.ProductionOrderWorkSchedule.Add(workSchedule);
            }
        }


        void IScheduling.ForwardScheduling(ProductionOrder productionOrder)
        {

        }

        void IScheduling.CapacityScheduling()
        {

        }
        
    }
}
