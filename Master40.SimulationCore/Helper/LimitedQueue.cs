using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Helper
{
    public class LimitedQueue<FWokrItem> : List<FWorkItem>
    {
        public int Limit { get; set; }
        public bool CapacitiesLeft => (Limit > this.Count) ? true : false;
        public LimitedQueue(int limit) : base(limit)
        {
            Limit = limit;
        }

        public void Enqueue(FWorkItem item)
        {
            if (Limit > this.Count)
                base.Add(item);
            else
                throw new Exception("Queue Limit Exeeds");
        }

        public FWorkItem Dequeue()
        {
            if (Count == 0) return null;

            var item = this.OrderBy(x => x.ItemPriority).FirstOrDefault();
            this.Remove(item);
            return item;

        }
    }
}
