using System;
using System.Collections.Generic;
using System.Linq;
using NSimulate;
using NSimulate.Instruction;

namespace NSimFactoryTest
{
    public class Machine : Process
    {
        private Random _random = null;
        private List<WorkSchedule> _unprocessedJobsList = null;
        private string _name;

        /// <summary>
        /// Gets the job queue for this machine
        /// </summary>
        /// <value>
        /// The job queue.
        /// </value>
        public Queue<WorkSchedule> JobQueue
        {
            get;
            private set;
        }



        public bool CheckIfParentCanBeQueued(WorkSchedule ws)
        {
            // get Parrent
            var parent = _unprocessedJobsList.FirstOrDefault(x => x.Id == ws.WorkScheduleId);
            var siblings = _unprocessedJobsList.Where(x => x.WorkScheduleId == ws.WorkScheduleId
                                                        && x.WorkScheduleId != x.Id);
            
            // check if all childs are ready
            var getRedy = siblings.Any(x => x.ItemState != ItemState.Ready);
            // if there are any return false
            if (getRedy)
                return false;

            // else return true 
            if (parent != null)
                JobQueue.Enqueue(parent);
            
            
            return true;
        }


        /// <summary>
        /// Gets the processed count.
        /// </summary>
        /// <value> The processed count. </value>
        public int ProcessedCount
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="NSimulate.Example1.Machine"/> class.
        /// </summary>
        /// <param name='jobQueue'> Job queue. </param>
        /// <param name='unprocessedJobsList'> Unprocessed jobs list. </param>
        public Machine(Queue<WorkSchedule> jobQueue, List<WorkSchedule> unprocessedJobsList, Random random, string name) : base()
        {
            _random = random;
            JobQueue = jobQueue;
            _unprocessedJobsList = unprocessedJobsList;
            this._name = name;
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
                    jobToProcess.ItemState = ItemState.Queued;
                    Console.WriteLine(Context.TimePeriod + " Start with Workschedule on " + this._name);
                    // simulate processing the job
                    // which takes time
                    yield return new WaitInstruction(jobToProcess.ProcessingTimeRequiredByJobQueue[JobQueue]);
                   {
                        ProcessedCount++;
                        // record the fact that the job has been processed by this machine type
                        jobToProcess.ProcessingTimeRequiredByJobQueue.Remove(JobQueue);

                        // if the job still requires other processing
                        if (jobToProcess.RequiresMoreWork)
                        {
                            // add it to the next queue
                            var something  = jobToProcess.ProcessingTimeRequiredByJobQueue.Keys.First();
                             something.Enqueue(jobToProcess);
                        }
                        else
                        {
                            // otherwise remove it from the all unprocessed jobs list
                            _unprocessedJobsList.Remove(jobToProcess);
                            CheckIfParentCanBeQueued(jobToProcess);
                            Console.WriteLine(Context.TimePeriod + " Finished with Workschedule after: " + jobToProcess.ProcessingTime + " min");
                        }
                    }
                }
            }
        }

        public bool CheckForRandomBreakdown()
        {
            bool isBrokenDown = false;

            var randomPercentage = _random.NextDouble() * 100;

            if (randomPercentage > 0.8)
            {
                isBrokenDown = true;
            }

            return isBrokenDown;
        }
    }
}