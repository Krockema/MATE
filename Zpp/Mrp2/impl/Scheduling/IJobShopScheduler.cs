using Zpp.Mrp2.impl.Scheduling.impl.JobShopScheduler;
using Zpp.Util.Graph;

namespace Zpp.Mrp2.impl.Scheduling
{
    public interface IJobShopScheduler
    {
        /**
         * Giffler-Thomson
         */
        void ScheduleWithGifflerThompsonAsZaepfel(IPriorityRule priorityRule,
            IDirectedGraph<INode> operationGraph);
    }
}