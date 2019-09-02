using Priority_Queue;
using Zpp.Common.DemandDomain;

namespace Zpp.Utils.Queue
{
    public class DemandQueueNode : FastPriorityQueueNode
    {
        private string _name;
        private readonly Demand _demand;

        public DemandQueueNode(Demand demand)
        {
            _demand = demand;
            _name = demand.ToString();
        }

        public Demand GetDemand()
        {
            return _demand;
        }

    }
}