using Priority_Queue;
using Zpp.DataLayer.impl.DemandDomain;

namespace Zpp.Util.Queue
{
    public class DemandQueueNode : FastPriorityQueueNode
    {
        private readonly Demand _demand;

        public DemandQueueNode(Demand demand)
        {
            _demand = demand;
        }

        public Demand GetDemand()
        {
            return _demand;
        }

    }
}