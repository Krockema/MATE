﻿using Akka.Actor;

namespace Mate.Production.Core.Agents.ProductionAgent.Types
{
    public class ArticleProvider
    {
        public IActorRef Provider { get; set; }
        public ArticleRecord Article { get; set; }

        public ArticleProvider(IActorRef provider, ArticleRecord article)
        {
            Provider = provider;
            Article = article;
        }
    }
}

