using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.Util;
using Zpp.Util.Graph;

namespace Zpp.Mrp2.impl.Scheduling.impl
{
    public class BackwardScheduler : IBackwardsScheduler
    {
        private readonly Stack<INode> _S;
        private readonly IDirectedGraph<INode> _orderOperationGraph;
        private readonly bool _clearOldTimes;

        public BackwardScheduler(Stack<INode> rootNodes, IDirectedGraph<INode> orderOperationGraph,
            bool clearOldTimes)
        {
            _S = rootNodes;
            _orderOperationGraph = orderOperationGraph;
            _clearOldTimes = clearOldTimes;
        }

        /**
         * Top-down
         */
        public void ScheduleBackward()
        {
            // S = {0} (alle einplanbaren Operations/Demands/Providers)

            if (_clearOldTimes)
            {
                // d_0 = 0
                foreach (var uniqueNode in _orderOperationGraph.GetAllUniqueNodes())
                {
                    IScheduleNode uniqueScheduleNode = uniqueNode.GetEntity();
                    if (uniqueScheduleNode.IsReadOnly() == false &&
                        uniqueScheduleNode.GetType() != typeof(CustomerOrderPart))
                    {
                        uniqueScheduleNode.ClearStartTimeBackward();
                        uniqueScheduleNode.ClearEndTimeBackward();
                    }
                }
            }

            // while S nor empty do
            while (_S.Any())
            {
                INode i = _S.Pop();
                IScheduleNode iAsScheduleNode = i.GetEntity();

                INodes successorNodes = _orderOperationGraph.GetSuccessorNodes(i);
                if (successorNodes != null && successorNodes.Any())
                {
                    foreach (var successor in successorNodes)
                    {
                        _S.Push(successor);
                        
                        IScheduleNode successorScheduleNode = successor.GetEntity();
                        if (successorScheduleNode.IsReadOnly())
                        {
                            continue;
                        }
                        
                        
                        // Konservativ vorwärtsterminieren ist korrekt,
                        // aber rückwärts muss wenn immer möglich terminiert werden
                        // (prüfe parents und ermittle minStart und setze das)
                        INodes predecessorNodes =
                            _orderOperationGraph.GetPredecessorNodes(successor);
                        DueTime minStartTime = iAsScheduleNode.GetStartTimeBackward();
                        if (minStartTime == null)
                        {
                            throw new MrpRunException(
                                "How can the StartTime of an already scheduled node be null ?");
                        }

                        foreach (var predecessorNode in predecessorNodes)
                        {
                            DueTime predecessorsStartTime =
                                predecessorNode.GetEntity().GetStartTimeBackward();
                            if (predecessorsStartTime != null &&
                                predecessorsStartTime.IsSmallerThan(minStartTime))
                            {
                                minStartTime = predecessorsStartTime;
                            }
                        }

                        if (successorScheduleNode.GetType() == typeof(CustomerOrderPart))
                        {
                            throw new MrpRunException(
                                "Only a root node can be a CustomerOrderPart.");
                        }

                        if (successorScheduleNode.IsReadOnly() == false)
                        {
                            successorScheduleNode.SetEndTimeBackward(minStartTime);
                        }
                    }
                }
            }
        }
    }
}