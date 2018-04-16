using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Master40.Agents.Agents.Model;
using Microsoft.EntityFrameworkCore.Storage;

namespace Master40.Agents.Agents.Internal
{
    public class LimitedQueue<TWorkItem> : List<WorkItem>
    {
        public int Limit { get; set; }
        public bool CapacitiesLeft => (Limit > this.Count) ? true : false;
        public LimitedQueue(int limit) : base(limit)
        {
            Limit = limit;
        }

        public void Enqueue(WorkItem item)
        {
            if (Limit > this.Count)
                base.Add(item);
            else
                throw new RetryLimitExceededException("Queue Limit Exeeds");
        }
        
        public WorkItem Dequeue()
        {
            if(Count == 0) return null;

            var item = this.OrderBy(x => x.Priority()).FirstOrDefault();
            this.Remove(item);
            return item;
        }
    }

}