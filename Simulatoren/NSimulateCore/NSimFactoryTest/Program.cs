using System;
using System.Collections.Generic;
using System.Linq;
using NSimulate;

namespace NSimFactoryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            // Make a simulation context
            using (var context = new SimulationContext(isDefaultContextForProcess: true))
            {
                // initialise the model
                var machines = CreateModel(context: context, numberOfJobs: 500);

                // instantate a new simulator
                var simulator = new Simulator();

                // run the simulation
                simulator.Simulate();

                Console.WriteLine("");
                Console.WriteLine("Day Over");
                Console.WriteLine("");
                Console.WriteLine("Jobs processed in {0} minutes", context.TimePeriod);

                int index = 1;
                foreach (var machine in machines)
                {
                    Console.WriteLine("Machine {0} processed {1} jobs.", index, machine.ProcessedCount);
                    index++;
                }
            }


            Console.ReadLine();
        }

        private static List<Machine> CreateModel(SimulationContext context, int numberOfJobs)
        {

            var rand = new Random();

            var unprocessedJobsList = new List<WorkSchedule>();

            // Create job queues of various work types
            var workTypeAJobQueue = new Queue<WorkSchedule>();
            //var workTypeBJobQueue = new Queue<WorkSchedule>();
            //var workTypeCJobQueue = new Queue<WorkSchedule>();
            var workQueues = new List<Queue<WorkSchedule>>() { workTypeAJobQueue }; //, workTypeBJobQueue, workTypeCJobQueue};

            var random = new Random();
            // create machines
            var machine1 = new Machine(jobQueue: workTypeAJobQueue, unprocessedJobsList: unprocessedJobsList, random: random, name: "M_1_");
            var machine2 = new Machine(jobQueue: workTypeAJobQueue, unprocessedJobsList: unprocessedJobsList, random: random, name: "M_2_");
            //var machine3 = new Machine(jobQueue:  workTypeBJobQueue, reliabilityPercentage: 99.0, repairTimeRequired: 15, unprocessedJobsList: unprocessedJobsList, random: random);
            //var machine4 = new Machine(jobQueue:  workTypeBJobQueue, reliabilityPercentage: 96.0, repairTimeRequired: 17, unprocessedJobsList: unprocessedJobsList, random: random);
            //var machine5 = new Machine(jobQueue:  workTypeCJobQueue, reliabilityPercentage: 98.0, repairTimeRequired: 20, unprocessedJobsList: unprocessedJobsList, random: random);
            var machines = new List<Machine>()
            {
                machine1,
                machine2,
            };

            // Create Bom
            for (var i = 0; i < numberOfJobs; i++)
            {
                var r = new Random();
                // create random Link to parrent
                var pa = r.Next(0, workQueues[0].Count);
                // for Deeper Trees
                if (r.Next(0,1) == 0 )
                {
                    pa = 0;
                }

                var s = new WorkSchedule()
                {
                    Id = i,
                    WorkScheduleId = pa,
                    Name = "Item: " + i + " Required by: " + pa + "!",
                    ItemState = ItemState.Ready
                };
                var duration = 14 + rand.Next(4);
                s.ProcessingTime = duration;
                s.ProcessingTimeRequiredByJobQueue[workTypeAJobQueue] = duration;
                // add to Unprocessed Job List and set Parrent to Created
                unprocessedJobsList.Add(s);
                unprocessedJobsList.FirstOrDefault(x => x.Id == s.WorkScheduleId).ItemState = ItemState.Created;
            }

            foreach (var item in unprocessedJobsList.Where(x => x.ItemState == ItemState.Ready))
            {
                // enqueue it 
                workQueues[0].Enqueue(item);
            }


            // add the end condition
            new SimulationEndTrigger(() => unprocessedJobsList.Count == 0);

            return machines;
        }
        private List<WorkSchedule> CreateInitialWorkSchedules()
        {
            List<WorkSchedule> wsl = new List<WorkSchedule>();
            

            return wsl;
        }
    }
}