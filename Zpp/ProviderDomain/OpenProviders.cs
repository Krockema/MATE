using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Zpp.Utils;

namespace Zpp.ProviderDomain
{
    public class OpenProviders
    {
        private readonly Dictionary<M_Article, List<OpenProvider>> _openProviders = new Dictionary<M_Article, List<OpenProvider>>();

        public void Add(M_Article article, OpenProvider openProvider)
        {
            InitArticle(article);
            if (AnyOpenProvider(article))
            {
                throw new MrpRunException($"Only one open provider is allowed: already open: \"{GetOpenProvider(article)}\" , cannot add: \"{openProvider}\"");
            }
            _openProviders[article].Add(openProvider);
        }

        public bool AnyOpenProvider(M_Article article)
        {
            InitArticle(article);
            return _openProviders[article].Any();
        }

        public OpenProvider GetOpenProvider(M_Article article)
        {
            if (AnyOpenProvider(article) == false)
            {
                return null;
            }

            return _openProviders[article][0];
        }

        public void Remove(OpenProvider provider)
        {
            _openProviders[provider.GetArticle()].RemoveAt(0);
        }

        private void InitArticle(M_Article article)
        {
            if (_openProviders.ContainsKey(article) == false)
            {
                _openProviders.Add(article, new List<OpenProvider>());
            }
        }
    }
}