using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Xunit;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp
{
    public class OrderGraph : IGraph<INode>
    {
        private readonly Dictionary<INode, List<INode>> _adjacencyList =
            new Dictionary<INode, List<INode>>();

        public OrderGraph(IDbTransactionData dbTransactionData)
        {
            foreach (var demandToProvider in dbTransactionData.DemandToProviderGetAll().GetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(new Id(demandToProvider.DemandId));
                Provider provider =
                    dbTransactionData.ProvidersGetById(new Id(demandToProvider.ProviderId));
                Assert.True(demand != null || provider != null,
                    "Demand/Provider should not be null.");
                AddEdge(new Node(demand, demandToProvider.GetDemandId()),
                    new Node(provider, demandToProvider.GetProviderId()));
            }

            foreach (var providerToDemand in dbTransactionData.ProviderToDemandGetAll().GetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(new Id(providerToDemand.DemandId));
                Provider provider =
                    dbTransactionData.ProvidersGetById(new Id(providerToDemand.ProviderId));
                Assert.True(demand != null || provider != null,
                    "Demand/Provider should not be null.");
                AddEdge(new Node(provider, providerToDemand.GetProviderId()),
                    new Node(demand, providerToDemand.GetDemandId()));
            }
        }

        public List<INode> GetToNodesOfFromNode(INode fromNode)
        {
            if (!_adjacencyList.ContainsKey(fromNode))
            {
                return null;
            }

            return _adjacencyList[fromNode];
        }

        public void AddEdges(INode fromNode, List<INode> toNodes)
        {
            if (!_adjacencyList.ContainsKey(fromNode))
            {
                _adjacencyList.Add(fromNode, toNodes);
                return;
            }

            _adjacencyList[fromNode].AddRange(toNodes);
        }

        public void AddEdge(INode fromNode, INode toNode)
        {
            if (!_adjacencyList.ContainsKey(fromNode))
            {
                _adjacencyList.Add(fromNode, new List<INode>());
            }

            _adjacencyList[fromNode].Add(toNode);
        }

        public int CountEdges()
        {
            return GetAllToNodes().Count;
        }

        public List<INode> GetAllNodes()
        {
            // TODO: use Set here
            List<INode> nodes = new List<INode>();
            nodes.AddRange(_adjacencyList.Keys.ToList());
            foreach (var nodeList in _adjacencyList.Values.ToList())
            {
                foreach (var node in nodeList)
                {
                    if (nodes.Contains(node) == false)
                    {
                        nodes.Add(node);
                    }    
                }
                
            }

            return nodes;
        }

        public override string ToString()
        {
            string mystring = "";
            foreach (var fromNode in _adjacencyList.Keys)
            {
                foreach (var toNode in _adjacencyList[fromNode])
                {
                    // TODO: <Type>, <Menge>, <ItemName>
                    mystring +=
                        $"\"{fromNode.GetId()};{fromNode.GetGraphizString()}\" -> " + 
                        $"\"{toNode.GetId()};{toNode.GetGraphizString()}\";" +
                        Environment.NewLine;
                }
            }

            return mystring;
        }

        private string enumToString(NodeType nodeType)
        {
            return Enum.GetName(typeof(NodeType), nodeType);
        }

        public List<INode> GetAllToNodes()
        {
           List<INode> toNodes = new List<INode>();

           foreach (var nodeList in _adjacencyList.Values.ToList())
           {
               toNodes.AddRange(nodeList);
           }

           return toNodes;
        }
    }
}