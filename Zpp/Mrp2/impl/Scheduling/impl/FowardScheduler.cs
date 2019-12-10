using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.Util.Graph;

namespace Zpp.Mrp2.impl.Scheduling.impl
{
    public class ForwardScheduler : IForwardScheduler
    {
        private readonly OrderOperationGraph _orderOperationGraph;
        private readonly SimulationInterval _simulationInterval;

        public ForwardScheduler(OrderOperationGraph orderOperationGraph, SimulationInterval simulationInterval)
        {
            _orderOperationGraph = orderOperationGraph;
            _simulationInterval = simulationInterval;
        }

        public void ScheduleForward()
        {
            Stack<INode> S = new Stack<INode>();

            // d_0 = 0
            foreach (var node in _orderOperationGraph.GetLeafNodes())
            {
                IScheduleNode scheduleNode = node.GetEntity();
                if (scheduleNode.IsReadOnly() == false && 
                    scheduleNode.GetStartTimeBackward().IsSmallerThan(_simulationInterval.GetStart()))
                {
                    // implicitly the due/endTime will also be set accordingly
                    scheduleNode.SetStartTimeBackward(_simulationInterval.GetStart());
                    S.Push(node);
                }
                else // no forward scheduling is needed
                {
                }
            }


            // while S nor empty do
            while (S.Any())
            {
                INode i = S.Pop();
                IScheduleNode iAsScheduleNode = (IScheduleNode) i.GetEntity();

                INodes predecessors = _orderOperationGraph.GetPredecessorNodes(i);
                if (predecessors != null && predecessors.Any())
                {
                    foreach (var predecessor in predecessors)
                    {
                        IScheduleNode predecessorScheduleNode = predecessor.GetEntity();
                        
                        // if predecessor starts before endTime of current d/p --> change that
                        if (predecessorScheduleNode.IsReadOnly() == false && predecessorScheduleNode
                                .GetStartTimeBackward().IsSmallerThan(iAsScheduleNode.GetEndTimeBackward()))
                        {
                            // COPs are not allowed to change
                            if (predecessorScheduleNode.GetType() != typeof(CustomerOrderPart))
                            {
                                // don't take getDueTime() since in case of a demand,
                                // this will be the startTime, which is to early

                                // This must be the maximum endTime of all childs !!!
                                DueTime maxEndTime = iAsScheduleNode.GetEndTimeBackward();
                                foreach (var successor in _orderOperationGraph.GetSuccessorNodes(
                                    predecessor))
                                {
                                    DueTime successorsEndTime = successor.GetEntity().GetEndTimeBackward();
                                    if (successorsEndTime.IsGreaterThan(maxEndTime))
                                    {
                                        maxEndTime = successorsEndTime;
                                    }
                                }

                                predecessorScheduleNode.SetStartTimeBackward(maxEndTime);
                            }
                        }

                        S.Push(predecessor);
                    }
                }
            }
        }
    }
}