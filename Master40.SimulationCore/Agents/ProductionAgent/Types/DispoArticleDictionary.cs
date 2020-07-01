using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using static FArticles;
using static FOperations;

namespace Master40.SimulationCore.Agents.ProductionAgent.Types
{
    public class DispoArticleDictionary
    {
        public List<ArticleProvider> DispoToArticleRelation = new List<ArticleProvider>();
        public FOperation Operation { get; set; }
        internal List<ArticleProvider> GetAll => DispoToArticleRelation.ToList();

        public DispoArticleDictionary(FOperation operation)
        {
            Operation = operation;

        }
        
        public void Add(IActorRef dispoRef, FArticle fArticle)
        {
            DispoToArticleRelation.Add(new ArticleProvider(dispoRef, fArticle));
        }

        internal bool AllProvided()
        {
            return DispoToArticleRelation.All(x => x.Article.IsProvided);
        }

        internal FArticle GetArticleByKey(Guid articleKey)
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
