using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Xunit;
using Zpp.DemandDomain;
using Zpp.GraphicalRepresentation;
using Zpp.MachineDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;

namespace Zpp
{
    public class DemandToProviderDirectedGraph : DirectedGraph, IDirectedGraph<INode>
    {
        

        public DemandToProviderDirectedGraph(IDbTransactionData dbTransactionData): base(dbTransactionData)
        {
            foreach (var demandToProvider in dbTransactionData.DemandToProviderGetAll().GetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(new Id(demandToProvider.DemandId));
                Provider provider =
                    dbTransactionData.ProvidersGetById(new Id(demandToProvider.ProviderId));
                Assert.True(demand != null || provider != null,
                    "Demand/Provider should not be null.");
                INode fromNode = new Node(demand, demandToProvider.GetDemandId());
                INode toNode = new Node(provider, demandToProvider.GetProviderId());
                AddEdge(fromNode, new Edge(demandToProvider, fromNode, toNode));
            }

            foreach (var providerToDemand in dbTransactionData.ProviderToDemandGetAll().GetAll())
            {
                Demand demand = dbTransactionData.DemandsGetById(new Id(providerToDemand.DemandId));
                Provider provider =
                    dbTransactionData.ProvidersGetById(new Id(providerToDemand.ProviderId));
                Assert.True(demand != null || provider != null,
                    "Demand/Provider should not be null.");

                INode fromNode = new Node(provider, providerToDemand.GetProviderId());
                INode toNode = new Node(demand, providerToDemand.GetDemandId());
                AddEdge(fromNode,
                    new Edge(providerToDemand.ToDemandToProvider(), fromNode, toNode));
            }
        }
    }
}