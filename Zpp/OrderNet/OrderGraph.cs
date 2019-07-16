using System;
using System.Collections.Generic;
using Zpp.DemandDomain;
using Zpp.Utils;

namespace Zpp
{
    public class OrderGraph : IGraph<INode>
    {
        private readonly Dictionary<INode, List<INode>> _adjacencyList =
            new Dictionary<INode, List<INode>>();

        public List<INode> GetChildNodes(INode node)
        {
            if (!_adjacencyList.ContainsKey(node))
            {
                return null;
            }

            return _adjacencyList[node];
        }

        public Dictionary<INode, List<INode>> GetAdjacencyList()
        {
            return _adjacencyList;
        }

        public void AddChilds(INode node, List<INode> nodes)
        {
            if (!_adjacencyList.ContainsKey(node))
            {
                _adjacencyList.Add(node, nodes);
                return;
            }

            _adjacencyList[node].AddRange(nodes);
        }

        public void AddChild(INode node, INode childNode)
        {
            if (!_adjacencyList.ContainsKey(node))
            {
                _adjacencyList.Add(node, new List<INode>());
            }

            _adjacencyList[node].Add(childNode);
        }

        public int Count()
        {
            return _adjacencyList.Values.Count;
        }

        public override string ToString()
        {
            string mystring = "";
            foreach (var fromNode in _adjacencyList.Keys)
            {
                foreach (var toNode in _adjacencyList[fromNode])
                {
                    mystring += $"\"{fromNode.GetId()}" +
                                $"{enumToString(fromNode.GetNodeType())[0]}\" -> \"{toNode.GetId()}" +
                                $"{enumToString(toNode.GetNodeType())[0]}\";" + Environment.NewLine;
                }
            }

            return mystring;
        }
        
        private string enumToString(NodeType nodeType)
        {
            return Enum.GetName(typeof(NodeType), nodeType);
        }
}

}