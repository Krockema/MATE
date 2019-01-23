using Master40.Agents.Agents.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.Agents.Agents.Model
{
    public class Batches : List<Batch>
    {
        public Batch GetByTool(string Tool)
        {
            return this.Single(x => x.BatchForTool == Tool);
        }

        /// <summary>
        /// Find an item by Guid in Batches
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Batch FindWorkItem(Guid guid)
        {
            foreach (var batch in this)
            {
                foreach (var item in batch)
                {
                    if (item.Id == guid)
                    {
                        return batch;
                    }
                }
            }
            return null;
        }

        public bool Add(WorkItem workItem)
        {
            Batch batch = this.GetByTool(workItem.WorkSchedule.MachineTool.Name);
            if (batch == null) return false;

            batch.Add(workItem);
            return true;
        }

        public Batch Dequeue(Guid guid, long currentTime)
        {
            double batchPrio = double.MaxValue;
            foreach (var item in this)
            {
                var t = item.Priority(currentTime);
                if (t < batchPrio)
                {
                    batchPrio = t;
                } 
            }
            var batch = this.FirstOrDefault(x => x.Priority() == batchPrio);
            this.Remove(batch);
            return batch;
        }

        public void Enqueue(WorkItem workItem)
        {
            var existing = this.FirstOrDefault(x => x.BatchForTool == workItem.WorkSchedule.MachineTool.Name);
            if (existing != null)
            {
                existing.Add(workItem);
            } else
            {
                this.Add(new Batch(workItem.WorkSchedule.MachineTool.Name) { workItem });
            }
        }
    }
}
