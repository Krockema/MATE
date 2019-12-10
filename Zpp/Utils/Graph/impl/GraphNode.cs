using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.Util.StackSet;

namespace Zpp.Util.Graph.impl
{
    /**
     * This is used in DirectedGraph internally to avoid exposing successors and predeccesors
     */
    internal class GraphNode: IGraphNode
    {
        private readonly INode _node;
        
        private readonly IStackSet<IGraphNode> _successors = new StackSet<IGraphNode>();
        private readonly IStackSet<IGraphNode> _predecessors = new StackSet<IGraphNode>();

        public GraphNode(INode node)
        {
            _node = node;
        }
        
        public override bool Equals(object obj)
        {
            GraphNode otherObject = (GraphNode) obj;
            // performance optimized
            return _node.GetId().GetValue() == otherObject._node.GetId().GetValue();
        }

        public override int GetHashCode()
        {
            // return HashCode.Combine(_id.GetHashCode(), _entity.GetEntity().GetNodeType().GetHashCode());
            return _node.GetId().GetHashCode();
        }

        public override string ToString()
        {
            return $"{_node.ToString()}";
        }

        public Id GetId()
        {
            return _node.GetId();
        }

        public void AddSuccessor(IGraphNode node)
        {
            _successors.Push(node);
        }

        public void AddSuccessors(GraphNodes nodes)
        {
            foreach (var node in nodes)
            {
                AddSuccessor(node);
            }
        }

        public GraphNodes GetSuccessors()
        {
            GraphNodes nodes = new GraphNodes();
            nodes.AddAll(_successors);
            return nodes;
        }

        public void AddPredecessor(IGraphNode node)
        {
            _predecessors.Push(node);
        }

        public void AddPredecessors(GraphNodes nodes)
        {
            foreach (var node in nodes)
            {
                AddPredecessor(node);
            }
        }

        public GraphNodes GetPredecessors()
        {
            GraphNodes nodes = new GraphNodes(_predecessors);
            return nodes;
        }

        public void RemoveSuccessor(IGraphNode node)
        {
            _successors.Remove(node);
        }

        public void RemovePredecessor(IGraphNode node)
        {
            _predecessors.Remove(node);
        }

        public void RemoveAllSuccessors()
        {
            _successors.Clear();
        }

        public void RemoveAllPredecessors()
        {
            _predecessors.Clear();
        }

        public INode GetNode()
        {
            return _node;
        }
    }
}