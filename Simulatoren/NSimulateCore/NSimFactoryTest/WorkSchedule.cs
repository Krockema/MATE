using System.Collections.Generic;
using NSimulate;

namespace NSimFactoryTest
{
    public class WorkSchedule : Process
    {
       
        public string Name { get; set; }
        public int Id { get; set; }
        public int WorkScheduleId { get; set; }
        public ItemState ItemState { get; set; }
        public virtual List<WorkSchedule> WorkSchedules { get; set; }
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