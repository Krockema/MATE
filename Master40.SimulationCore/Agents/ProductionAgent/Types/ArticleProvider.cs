using System;
using System.Collections.Generic;
using System.Text;
using Akka.Actor;
using Master40.DB.DataModel;
using static FArticles;

namespace Master40.SimulationCore.Agents.ProductionAgent.Types
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

