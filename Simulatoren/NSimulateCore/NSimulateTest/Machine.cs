using System;
using NSimulate;
using NSimulate.Instruction;
using System.Collections.Generic;
using System.Linq;

namespace NSimJobTest
{
    /// <summary>
    /// A Machine that performs work on jobs
    /// </summary>
    public class Machine : Process
    {
        private Random _random = null;
        private List<Job> _unprocessedJobsList = null;

        /// <summary>
        /// Gets the job queue for this machine
        /// </summary>
        /// <value>
        /// The job queue.
        /// </value>
        public Queue<Job> JobQueue
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reliability percentage used when checking for breakdowns.
        /// </summary>
        /// <value>
        /// The reliability percentage.
        /// </value>
        public double ReliabilityPercentage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the repair time required to fix this machine when it breaks down.
        /// </summary>
        /// <value>
        /// The repair time required.
        /// </value>
        public int RepairTimeRequired
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the processed count.
        /// </summary>
        /// <value>
        /// The processed count.
        /// </value>
        public int ProcessedCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the count of breakdowns
        /// </summary>
        /// <value>
        /// The breakdown count.
        /// </value>
        public int BreakdownCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NSimulate.Example1.Machine"/> class.
        /// </summary>
        /// <param name='jobQueue'>
        /// Job queue.
        /// </param>
        /// <param name='reliabilityPercentage'>
        /// Reliability percentage.
        /// </param>
        /// <param name='repairTimeRequired'>
        /// Repair time required.
        /// </param>
        /// <param name='unprocessedJobsList'>
        /// Unprocessed jobs list.
        /// </param>
        public Machine(Queue<Job> jobQueue,
            double reliabilityPercentage,
            int repairTimeRequired,
            List<Job> unprocessedJobsList,
            Random random)
            : base()
        {
            _random = random;
            JobQueue = jobQueue;
            ReliabilityPercentage = reliabilityPercentage;
            RepairTimeRequired = repairTimeRequired;
            _unprocessedJobsList = unprocessedJobsList;
        }

        /// <summary>
        /// Simulate this process
        /// </summary>
        public override IEnumerator<InstructionBase> Simulate()
        {
            // while the simulation is running
            while (true)
            {
                // check if the queue for this machine is empty
                if (JobQueue.Count == 0)
                {
                    // if it is, wait until there is something in the queue
                    yield return new WaitConditionInstruction(() => JobQueue.Count > 0);
                }
                else
                {
                    // take a job from the queue
                    var jobToProcess = JobQueue.Dequeue();

                    // simulate processing the job
                    // which takes time
                    yield return new WaitInstruction(jobToProcess.ProcessingTimeRequiredByJobQueue[JobQueue]);

                    // use the reliability indicator to determine if the machine is broken down
                    if (CheckForRandomBreakdown())
                    {
                        BreakdownCount++;
                        // the machine has broken down
                        // add the job it was processing back to the queue
                        JobQueue.Enqueue(jobToProcess);

                        // obtain a repair person
                        var allocateInstruction = new AllocateInstruction<RepairPerson>(1);
                        yield return allocateInstruction;

                        // and wait for the machine to be fixed
                        yield return new WaitInstruction(RepairTimeRequired);

                        // then release the repair person resource
                        yield return new ReleaseInstruction<RepairPerson>(allocateInstruction);

                    }
                    else
                    {
                        ProcessedCount++;
                        // record the fact that the job has been processed by this machine type
                        jobToProcess.ProcessingTimeRequiredByJobQueue.Remove(JobQueue);

                        // if the job still requires other processing
                        if (jobToProcess.RequiresMoreWork)
                        {
                            // add it to the next queue
                            jobToProcess.ProcessingTimeRequiredByJobQueue.Keys.First().Enqueue(jobToProcess);
                        }
                        else
                        {
                            // otherwise remove it from the all unprocessed jobs list
                            _unprocessedJobsList.Remove(jobToProcess);
                        }
                    }
                }
            }
        }

        public bool CheckForRandomBreakdown()
        {
            bool isBrokenDown = false;

            var randomPercentage = _random.NextDouble() * 100;

            if (randomPercentage > ReliabilityPercentage)
            {
                isBrokenDown = true;
            }

            return isBrokenDown;
        }
    }
}

