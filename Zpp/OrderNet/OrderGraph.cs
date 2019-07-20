using System;
using System.Collections.Generic;
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
                AddChild(new Node(demand, demandToProvider.GetDemandId()),
                    new Node(provider, demandToProvider.GetProviderId()));
            }

            foreach (var providerToDemand in dbTransactionData.ProviderToDemandGetAll().GetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(new Id(providerToDemand.DemandId));
                Provider provider =
                    dbTransactionData.ProvidersGetById(new Id(providerToDemand.ProviderId));
                Assert.True(demand != null || provider != null,
                    "Demand/Provider should not be null.");
                AddChild(new Node(provider, providerToDemand.GetProviderId()),
                    new Node(demand, providerToDemand.GetDemandId()));
            }
        }

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
    }
}