using Priority_Queue;
using Zpp.DemandDomain;

namespace Priority_Queue_Example
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