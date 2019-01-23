using Master40.Agents.Agents.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.Agents.Agents.Model
{
    public class Batch : List<WorkItem> , IProcessingItem
    {
        public Batch(string requiredTool)
        {
            BatchForTool = requiredTool;
            EstimatedStart = 0;
            Id = Guid.NewGuid();
            _priority = 0;
        }

        public Guid Id { get; }
        public string BatchForTool { get; }
        private double _priority { get; set; }
        public Guid MachineAgentId { get; set; }
        public List<Proposal> Proposals { get; set; }
        public long EstimatedStart { get; set; }
        public long EstimatedEnd { get; set; }
        /// <summary>
        /// Returns the Priority of the MostImportant WorkItem
        /// </summary>
        /// <param name="now">Simulation Time or Simulation Time</param>
        /// <returns></returns>
        public double Priority(long now) {
            _priority = this.OrderBy(x => x.Priority(now)).First().Priority();
            return _priority;
        }
        public double Priority()
        {
           return _priority;
        }

        public WorkItem GetMostImportant(long now)
        {
            return this.OrderBy(x => x.Priority(now)).First();
        }
         
        public bool Update(WorkItem workItem)
        {
            var toRemove = this.Single(x => x.Id == workItem.Id);
            this.Remove(toRemove);
            this.Add(workItem);
            return true;
        }

        public void SetEstimatedStart(long start)
        {
            this.EstimatedStart = start;
            this.EstimatedEnd = this.EstimatedStart + this.Sum(x => x.WorkSchedule.Duration);
        }

        public Status GetBatchStatus()
        {
            var status = Status.InQueue;
            this.ForEach(x => { if (x.MaterialsProvided == true) status = Status.Ready; });
            return status;
        }
    }
}
