using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;

namespace Mate.Production.Core.Agents.ProductionAgent.Types
{
    public class DispoArticleDictionary
    {
        private List<ArticleProvider> DispoToArticleRelation = new List<ArticleProvider>();
        public OperationRecord Operation { get; set; }
        internal List<ArticleProvider> GetAll => DispoToArticleRelation.ToList();

        public DispoArticleDictionary(OperationRecord operation)
        {
            Operation = operation;
        }
        
        public List<IActorRef> GetProviderList => 
             DispoToArticleRelation.Select(x => x.Provider).ToList();
        

        public void Add(IActorRef dispoRef, ArticleRecord fArticle)
        {
            DispoToArticleRelation.Add(new ArticleProvider(dispoRef, fArticle));
        }

        public void Add(ArticleProvider provider)
        {
            DispoToArticleRelation.Add(provider);
        }


        internal bool AllProvided()
        {
            return DispoToArticleRelation.All(x => x.Article.IsProvided);
        }

        internal ArticleRecord GetArticleByKey(Guid articleKey)
        {
            return DispoToArticleRelation.Single(x => x.Article.Key == articleKey).Article;
        }

        internal ArticleProvider GetByKey(Guid articleKey)
        {
            return DispoToArticleRelation.FirstOrDefault(x => x.Article.Key == articleKey);
        }

        internal List<ArticleProvider> GetWithoutProvider()
        {
            return DispoToArticleRelation.Where(x => x.Provider == ActorRefs.Nobody).ToList();
        }
    }
}
