using System.Collections.Generic;
using System.Linq;
using static IJobs;

namespace Mate.Production.Core.Agents.HubAgent.Types.Queuing
{

    public class JobQueue : List<IJob>
    {
        public IJob PeekNext(long currentTime)
        {
            return this.OrderBy(x => x.Priority(currentTime)).First();
        }

        public IJob DequeueNext(long currentTime)
        {
            var jobToDeuque = this.OrderBy(x => x.Priority(currentTime)).First();
            Remove(jobToDeuque);
            return jobToDeuque;
        }
        public double Priority(long currentTime)
        {
            return this.Min(x => x.Priority(currentTime));
        }

    }


    /*
    static class JobComparer
    {
        public static int CompareTo(this IJob pre, IJob other, long currentTime)
        {
            var prePrio = pre.Priority(currentTime);
            var otherPrio = other.Priority(currentTime);
            if (prePrio < otherPrio) return -1;
            if (prePrio == otherPrio) return 0;
            return +1;
        }

    }

    public class JobQueue 
    {
        public List<IJob> data = new List<IJob>();
        public void Enqueue(IJob item, long currentTime)
        {
            data.Add(item);
            int ci = data.Count - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (data[ci].CompareTo(data[pi], currentTime) >= 0) break; // child item is larger than (or equal) parent so we're done
                IJob tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }

        public IJob Dequeue(long currentTime)
        {
            // assumes pq is not empty; up to calling code
            int li = data.Count - 1; // last index (before removal)
            IJob frontItem = data[0];   // fetch the front
            data[0] = data[li];
            data.RemoveAt(li);

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && data[rc].CompareTo(data[ci], currentTime) < 0)  // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (data[pi].CompareTo(data[ci], currentTime) <= 0) break; // parent is smaller than (or equal to) smallest child so done
                IJob tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
                pi = ci;
            }
            return frontItem;
        }

        public IJob Peek()
        {
            IJob frontItem = data[0];
            return frontItem;
        }

        public IJob PeekNext(long currentTime)
        {
            return this.Peek();
        }

        public IJob DequeueNext(long currentTime)
        {
            return this.Dequeue(currentTime);
        }
        public double Priority(long currentTime)
        {
            return this.data.Min(x => x.Priority(currentTime));
        }
     */

}
