using System;
using System.Collections.Generic;
using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using Microsoft.FSharp.Collections;
using static FArticles;
using static FOperations;
using static FStockProviders;

namespace Master40.XUnitTest.Online.Preparations
{
    public static class TypeFactory
    {
        public static FOperation CreateDummyJobItem(string jobName, int jobDuration, int averageTransitionDuration = 20, bool preCondition = true, bool materialsProvide = true, 
                                                    int dueTime = 50, long customerDue = 100L, M_ResourceCapability capability = null,
                                                    M_ArticleBom bom = null, long currentTime = 0L)
        {
            var operation = new M_Operation()
            {
                ArticleId = 10,
                AverageTransitionDuration = averageTransitionDuration,
                Duration = jobDuration,
                HierarchyNumber = 10,
                Id = 1,
                Name = jobName,
                ArticleBoms = new List<M_ArticleBom> {bom},
                ResourceCapability = capability
            };
            return MessageFactory.ToOperationItem(operation, dueTime: dueTime, customerDue: customerDue, productionAgent: ActorRefs.Nobody, firstOperation: preCondition, currentTime: currentTime, remainingWork: 0);
        }

        public static FArticle CreateDummyArticle(int dueTime, long customerDue, int currentTime, M_Article article, int quantity)
        {
            return new FArticle(
                key: Guid.Empty
                , dueTime: dueTime
                , customerDue: customerDue
                , keys: new FSharpSet<Guid>(new Guid[] { })
                , quantity: quantity
                , article: article
                , creationTime: currentTime
                , customerOrderId: 0
                , isHeadDemand: true
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , isProvided: false
                , providedAt: 0
                , originRequester: ActorRefs.Nobody
                , dispoRequester: ActorRefs.Nobody
                , providerList: new List<FStockProvider>()
                , finishedAt: 0
                ,remainingDuration: 0
            );
        }
    }
}