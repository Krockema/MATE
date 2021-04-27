using System;
using System.Collections.Generic;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.HubAgent.Types;
using Mate.Production.Core.Helper;
using Microsoft.FSharp.Collections;
using static FArticles;
using static FJobConfirmations;
using static FOperations;
using static FStockProviders;

namespace Mate.Test.Online.Preparations
{
    public static class TypeFactory
    {
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
                , remainingDuration: 0
            ).CreateProductionKeys.SetPrimaryKey;
        }
        public static FOperation CreateDummyFOperationItem(string jobName, int jobDuration, int averageTransitionDuration = 20, bool preCondition = true, bool materialsProvide = true,
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
                ArticleBoms = new List<M_ArticleBom> { bom },
                ResourceCapability = new M_ResourceCapability { Name = "Cutting" }
            };
            return MessageFactory.ToOperationItem(
                priorityRule: PriorityRule.LST,
                m_operation: operation, 
                dueTime: dueTime,
                customerDue: customerDue,
                productionAgent: ActorRefs.Nobody,
                firstOperation: preCondition,
                currentTime: currentTime,
                remainingWork: 0,
                articleKey: Guid.NewGuid());
        }



        public static FJobConfirmation CreateDummyFJobConfirmations(int currentTime, int duration, long bucketSize)
        {
            var jobConfirmation = new JobConfirmation(
                MessageFactory.ToBucketScopeItem(
                    CreateDummyFOperationItem("Operation1", duration),
                    ActorRefs.NoSender,
                    currentTime,
                    bucketSize));

            return jobConfirmation.ToImmutable() as FJobConfirmation;

        }


        public static ProposalForCapabilityProvider CreateDummyProposalForCapabilityProvider()
        {
            M_Resource OperatorResource = new M_Resource { Name = "Operator", IResourceRef = "Operator" };
            M_Resource MachineResource = new M_Resource { Name = "Machine", IResourceRef = "Machine" };
            M_Resource WorkerResource = new M_Resource { Name = "Worker", IResourceRef = "Worker" };
            M_Resource MachineResource2 = new M_Resource { Name = "Machine2", IResourceRef = "Machine2" };

            var _proposalForCapabilityProvider
                = new ProposalForCapabilityProvider(
                    new M_ResourceCapabilityProvider
                    {
                        Name = "TestCapability",
                        ResourceSetups = new List<M_ResourceSetup>
                        {
                        new M_ResourceSetup {Name = "Operator", UsedInSetup = true, UsedInProcess = false, Resource = OperatorResource },
                        new M_ResourceSetup {Name = "Machine", UsedInSetup = true, UsedInProcess = true , Resource = MachineResource},
                        new M_ResourceSetup {Name = "Worker", UsedInSetup = false, UsedInProcess = true, Resource = WorkerResource },
                        new M_ResourceSetup {Name = "Machine2", UsedInSetup = true, UsedInProcess = true , Resource = MachineResource2},
                        }
                    });
            return _proposalForCapabilityProvider;
        }

    }
}