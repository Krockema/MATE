using System;
using System.Collections.Generic;
namespace NSimJobTest
{
    /// <summary>
    /// A job to be worked on using workshop machines
    /// </summary>
    public class Job
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NSimulate.Example1.Job"/> class.
        /// </summary>
        public Job()
        {
            ProcessingTimeRequiredByJobQueue = new Dictionary<Queue<Job>, int>();
        }




        /// <summary>
        /// Gets the processing time required to process this job by each job queue.
        /// </summary>
        /// <value>
        /// The processing time required to process this job by job queue that this job must go through
        /// </value>
        public Dictionary<Queue<Job>, int> ProcessingTimeRequiredByJobQueue
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="NSimulate.Example1.Job"/> requires more work.
        /// </summary>
        /// <value>
        /// <c>true</c> if requires more work; otherwise, <c>false</c>.
        /// </value>
        public bool RequiresMoreWork
        {
            get
            {
                return ProcessingTimeRequiredByJobQueue.Count > 0;
            }
        }
    }
}

