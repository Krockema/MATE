using System.Collections.Generic;
using NSimAgentTest.Enums;
using NSimulate;

namespace NSimAgentTest
{
    public class DBContext
    {
        public DBContext()
        {
            Machines = new List<string> { "SAW", "DRILL", "ASSEMBLY" };
            StockElements = new List<StockElement>
            {
                new StockElement { Name="Kipper", Quantity = 1},
                new StockElement { Name="Chassi", Quantity = 0 },
                new StockElement { Name="Bodenplatte", Quantity = 0 },
                new StockElement { Name="Räder", Quantity = 4 }
            };

            ProductionOrder = new List<ProductionOrder>
            {
                new ProductionOrder{
                    Id = 1,
                    ItemState = Status.Created,
                    ParrentId = 1,
                    Name = "Kipper",
                    CumulatedProcessingTime = 0,
                    WorkSchedules = new List<WorkSchedule> { new WorkSchedule { Id = 1, Name = "ASSEMBLY", ItemState = Status.Created, ProcessingTime = 15 } }
                },
                new ProductionOrder{
                    Id = 2,
                    ItemState = Status.Created,
                    ParrentId = 1,
                    Name = "Bodenplatte",
                    CumulatedProcessingTime = 0,
                    WorkSchedules = new List<WorkSchedule> { new WorkSchedule { Id = 1, Name = "SAW", ItemState = Status.Created, ProcessingTime = 15 } }
                },
                new ProductionOrder{
                    Id = 3,
                    ItemState = Status.Created,
                    ParrentId = 2,
                    Name = "Räder",
                    CumulatedProcessingTime = 0,
                    WorkSchedules = new List<WorkSchedule> { new WorkSchedule { Id = 1, Name = "DRILL", ItemState = Status.Created, ProcessingTime = 15 } }
                },
                new ProductionOrder{
                    Id = 3,
                    ItemState = Status.Created,
                    ParrentId = 1,
                    Name = "Chassi",
                    CumulatedProcessingTime = 0,
                    WorkSchedules = new List<WorkSchedule> { new WorkSchedule { Id = 1, Name = "ASSEMBLY", ItemState = Status.Created, ProcessingTime = 15 } }
                },
            };


            OrderList = new List<RequestItem>()
            {
                new RequestItem {Name = "First Order", Quantity = 2, Article = "Kipper" },
                new RequestItem {Name = "Secont Order", Quantity = 1, Article = "Kipper"},
            };


        }


        public List<string> Machines { get; set; }
        public List<StockElement> StockElements { get; set; }

        public List<RequestItem> OrderList { get; set; }
        public List<ProductionOrder> ProductionOrder { get; set; }


    }

    public class StockElement {
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

    public class RequestItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Article { get; set; }
    }


    public class ProductionOrder
    {
        public string Name { get; set; }
        public string MachineGroup { get; set; }
        public int Id { get; set; }
        public int ParrentId { get; set; }
        public Status ItemState { get; set; }
        public int CumulatedProcessingTime { get; set; }
        public List<WorkSchedule> WorkSchedules { get; set; }
    }



    public class WorkSchedule : Process
    {

        public string Name { get; set; }
        public int Id { get; set; }
        public Status ItemState { get; set; }
        public int ProcessingTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public WorkSchedule()
        {
            ProcessingTimeRequiredByJobQueue = new Dictionary<Queue<WorkSchedule>, int>();
        }

        /// <summary>
        /// Gets the processing time required to process this job by each job queue.
        /// </summary>
        /// <value>  The processing time required to process this job by job queue that this job must go through </value>
        public Dictionary<Queue<WorkSchedule>, int> ProcessingTimeRequiredByJobQueue
        {
            get; private set;
        }

        /// <summary>
        /// Gets a value indicating whether this requires more work.
        /// </summary>
        /// <value> <c>true</c> if requires more work; otherwise, <c>false</c>. </value>
        public bool RequiresMoreWork => ProcessingTimeRequiredByJobQueue.Count > 0;
    }

}