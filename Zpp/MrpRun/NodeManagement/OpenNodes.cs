using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Zpp.Common.DemandDomain.Wrappers;
using Zpp.Utils;

namespace Zpp.MrpRun.NodeManagement
{
    public class OpenNodes<T>
    {
        private readonly Dictionary<M_Article, List<OpenNode<T>>> _openNodes = new Dictionary<M_Article, List<OpenNode<T>>>();

        public void Add(M_Article article, OpenNode<T> openNode)
        {
            if (openNode.GetOpenNode().GetType() != typeof(StockExchangeDemand))
            {
                throw new MrpRunException("An open provider can only be a StockExchangeDemand.");
            }
            InitOpenProvidersDictionary(article);
            if (AnyOpenProvider(article))
            {
                throw new MrpRunException($"Only one open provider is allowed: already open: \"{GetOpenProvider(article)}\" , cannot add: \"{openNode}\"");
            }
            _openNodes[article].Add(openNode);
        }

        public bool AnyOpenProvider(M_Article article)
        {
            InitOpenProvidersDictionary(article);
            return _openNodes[article].Any();
        }

        public OpenNode<T> GetOpenProvider(M_Article article)
        {
            if (AnyOpenProvider(article) == false)
            {
                return null;
            }

            return _openNodes[article][0];
        }

        public void Remove(OpenNode<T> node)
        {
            _openNodes[node.GetArticle()].RemoveAt(0);
        }

        private void InitOpenProvidersDictionary(M_Article article)
        {
            if (_openNodes.ContainsKey(article) == false)
            {
                _openNodes.Add(article, new List<OpenNode<T>>());
            }
        }
    }
}