using Akka.Actor;
using static FArticles;

namespace Mate.Production.Core.Agents.ProductionAgent.Types
{
    public class ArticleProvider
    {
        public IActorRef Provider { get; set; }
        public FArticle Article { get; set; }

        public ArticleProvider(IActorRef provider, FArticle article)
        {
            Provider = provider;
            Article = article;
        }
    }
}

