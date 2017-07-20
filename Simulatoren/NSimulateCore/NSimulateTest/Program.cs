using System;
using System.Collections.Generic;
using NSimulate;
using NSimulate.Instruction;

namespace NSimJobTest
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

                Console.WriteLine("Jobs processed in {0} minutes", context.TimePeriod);

                int index = 1;
                foreach (var machine in machines)
                {
                    Console.WriteLine("Machine {0} processed {1} jobs and had {2} breakdowns.", index, machine.ProcessedCount, machine.BreakdownCount);
                    index++;
                }
            }
            Console.ReadLine();
        }
        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <param name='numberOfJobs'>
        /// Number of jobs to be generated.
        /// </param>
        private static List<Machine> CreateModel(SimulationContext context, int numberOfJobs)
        {

            var rand = new Random();

            var unprocessedJobsList = new List<Job>();

            // Create job queues of various work types
            var workTypeAJobQueue = new Queue<Job>();
            var workTypeBJobQueue = new Queue<Job>();
            var workTypeCJobQueue = new Queue<Job>();
            var workQueues = new List<Queue<Job>>() { workTypeAJobQueue, workTypeBJobQueue, workTypeCJobQueue };

            var random = new Random();
            // create machines
            var machine1 = new Machine(jobQueue: workTypeAJobQueue, reliabilityPercentage: 95.0, repairTimeRequired: 15, unprocessedJobsList: unprocessedJobsList, random: random);
            var machine2 = new Machine(jobQueue: workTypeAJobQueue, reliabilityPercentage: 85.0, repairTimeRequired: 22, unprocessedJobsList: unprocessedJobsList, random: random);
            var machine3 = new Machine(jobQueue: workTypeBJobQueue, reliabilityPercentage: 99.0, repairTimeRequired: 15, unprocessedJobsList: unprocessedJobsList, random: random);
            var machine4 = new Machine(jobQueue: workTypeBJobQueue, reliabilityPercentage: 96.0, repairTimeRequired: 17, unprocessedJobsList: unprocessedJobsList, random: random);
            var machine5 = new Machine(jobQueue: workTypeCJobQueue, reliabilityPercentage: 98.0, repairTimeRequired: 20, unprocessedJobsList: unprocessedJobsList, random: random);

            var machines = new List<Machine>()
            {
                machine1,
                machine2,
                machine3,
                machine4,
                machine5
            };

            // create the jobs
            for (int i = 0; i < numberOfJobs; i++)
            {
                var newJob = new Job();

                newJob.ProcessingTimeRequiredByJobQueue[workTypeAJobQueue] = 5 + rand.Next(5);
                newJob.ProcessingTimeRequiredByJobQueue[workTypeBJobQueue] = 5 + rand.Next(5);
                newJob.ProcessingTimeRequiredByJobQueue[workTypeCJobQueue] = 5 + rand.Next(5);

                int index = rand.Next(workQueues.Count);
                // enque the job in one of the work queues
                workQueues[index].Enqueue(newJob);

                // and add it to the unprocessed job list
                unprocessedJobsList.Add(newJob);
            }

            // add a repair person
            new RepairPerson() { Capacity = 1 };

            // add the end condition
            new SimulationEndTrigger(() => unprocessedJobsList.Count == 0);

            return machines;
        }

    }
}
