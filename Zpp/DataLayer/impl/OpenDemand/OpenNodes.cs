using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.Wrappers;
using Zpp.Util;
using Zpp.Util.StackSet;

namespace Zpp.DataLayer.impl.OpenDemand
{
    public class OpenNodes<T> where T: IId
    {
        private readonly Dictionary<M_Article, IStackSet<OpenNode<T>>> _openNodes = new Dictionary<M_Article, IStackSet<OpenNode<T>>>();

        public void Add(M_Article article, OpenNode<T> openNode)
        {
            if (openNode.GetOpenNode().GetType() != typeof(StockExchangeDemand))
            {
                throw new MrpRunException("An open provider can only be a StockExchangeDemand.");
            }
            InitOpenProvidersDictionary(article);
            _openNodes[article].Push(openNode);
        }

        public bool AnyOpenProvider(M_Article article)
        {
            InitOpenProvidersDictionary(article);
            return _openNodes[article].Any();
        }

        public IEnumerable<OpenNode<T>> GetOpenProvider(M_Article article)
        {
            if (AnyOpenProvider(article) == false)
            {
                return null;
            }

            return _openNodes[article];
        }

        public void Remove(OpenNode<T> node)
        {
            _openNodes[node.GetArticle()].Remove(node);
        }
        
        public void Remove(Demand demand)
        {
            _openNodes[demand.GetArticle()].RemoveById(demand.GetId());
        }
        
        public bool Contains(Demand demand)
        {
            return _openNodes[demand.GetArticle()].Contains(demand.GetId());
        }

        private void InitOpenProvidersDictionary(M_Article article)
        {
            if (_openNodes.ContainsKey(article) == false)
            {
                _openNodes.Add(article, new StackSet<OpenNode<T>>());
            }
        }

        public void Clear()
        {
            _openNodes.Clear();
        }

       
    }
}