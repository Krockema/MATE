﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static IJobs;

namespace Master40.SimulationCore.Agents.ResourceAgent.Types
{
    public abstract class LimitedQueue
    {
        internal List<IJob> jobs = new List<IJob>();
        public int Limit { get; set; }
        public int Count => jobs.Count;

        public LimitedQueue(int limit)
        {
            Limit = limit;
        }

        /// <summary>
        /// Checks if there is an item ready to produce and removes it from queue and returns it to caller
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public virtual IJob DequeueFirstSatisfied(long currentTime)
        {
            var item = this.jobs.Where(x => x.StartConditions.Satisfied).OrderBy(keySelector: x => x.Priority(currentTime)).FirstOrDefault();
            if (item != null)
            {
                this.jobs.Remove(item: item);
            }
            return item;
        }

        internal List<IJob> GetAllSatisfiedSameTool(long currentTime)
        {
            var job = DequeueFirstSatisfied(currentTime);
            var list = this.jobs.Where(x => x.StartConditions.Satisfied && x.Tool.Id == job.Tool.Id).ToList();
            foreach(var item in list)
            {
                this.jobs.Remove(item: item);
            }
            list.Add(job);
            return list;
        }

        public abstract bool CapacitiesLeft();

        public virtual bool HasQueueAbleJobs()
        {
            return this.jobs.Any(x => x.StartConditions.Satisfied);
        }

    }

}
