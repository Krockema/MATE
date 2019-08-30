using Akka.Actor;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using System;
using System.Collections.Generic;
using Master40.Simulation.CLI.Arguments;
using static FArticles;
using static FOperations;

namespace Master40.XUnitTest.Preparations
{
    public static class TypeFactory
    {
        public static FOperation CreateJobItem(string jobName, int jobDuration, bool preCondition = true, int dueTime = 50, string skillName = "Sewing", M_ArticleBom bom = null)
        {
            var operation = new M_Operation()
            {
                ArticleId = 10,
                AverageTransitionDuration = 20,
                Duration = jobDuration,
                HierarchyNumber = 10,
                Id = 1,
                Name = jobName,
                ArticleBoms = new List<M_ArticleBom> { bom },
                ResourceSkill = new M_ResourceSkill() { Name = skillName }
            };
            return operation.ToOperationItem(dueTime: 50, productionAgent: ActorRefs.Nobody, firstOperation: preCondition, currentTime: 0);
        }

        public static FArticle CreateDummyArticle(int dueTime, int currentTime, M_Article article, int quantity)
        {
            return new FArticle(
                key: Guid.NewGuid()
                , dueTime: dueTime
                , quantity: quantity
                , article: article
                , creationTime: currentTime
                , customerOrderId: 0
                , isHeadDemand: true
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , isProvided: false
                , originRequester: ActorRefs.Nobody
                , dispoRequester: ActorRefs.Nobody
                , providerList: new List<Guid>()
                , finishedAt: 0
            );
        }
    }
}