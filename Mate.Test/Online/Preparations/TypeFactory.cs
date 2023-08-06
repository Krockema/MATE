using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Hive.Definitions;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal;
using Mate.Production.Core.Agents.HubAgent.Types;
using Mate.Production.Core.Environment.Records.Scopes;
using Mate.Production.Core.Helper;

namespace Mate.Test.Online.Preparations
{
    public static class TypeFactory
    {
        public static ArticleRecord CreateDummyArticle(DateTime dueTime, DateTime customerDue, DateTime currentTime, M_Article article, int quantity)
        {
            return new ArticleRecord(
                //# Key: Guid.Empty
                DueTime: dueTime
                , CustomerDue: customerDue
                , Keys: ImmutableHashSet.Create<Guid>()
                , Quantity: quantity
                , Article: article
                , CreationTime: currentTime
                , CustomerOrderId: 0
                , IsHeadDemand: true
                , StockExchangeId: Guid.Empty
                , StorageAgent: ActorRefs.NoSender
                , IsProvided: false
                , ProvidedAt: Time.ZERO.Value
                , OriginRequester: ActorRefs.Nobody
                , DispoRequester: ActorRefs.Nobody
                , ProviderList: ImmutableHashSet.Create<StockProviderRecord>()
                , FinishedAt: Time.ZERO.Value
                , RemainingDuration: TimeSpan.Zero
            );
        }
        public static OperationRecord CreateDummyOperationRecord(string jobName, TimeSpan jobDuration, TimeSpan averageTransitionDuration, DateTime dueTime, DateTime customerDue, DateTime currentTime
                                                                , bool preCondition = true, bool materialsProvide = true, M_ResourceCapability capability = null,
                                                                M_ArticleBom bom = null)
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
            return MessageFactory.ToOperationRecord(
                priorityRule: PriorityRule.LST,
                m_operation: operation, 
                dueTime: dueTime,
                customerDue: customerDue,
                productionAgent: ActorRefs.Nobody,
                firstOperation: preCondition,
                currentTime: currentTime,
                remainingWork: TimeSpan.Zero,
                articleKey: Guid.NewGuid());
        }



        public static JobConfirmationRecord CreateDummyJobConfirmations(DateTime currentTime, TimeSpan duration, TimeSpan bucketSize)
        {
            var jobConfirmation = new JobConfirmation(
                MessageFactory.ToBucketScopeItem(
                    CreateDummyOperationRecord(jobName: "Operation1"
                                               ,jobDuration: duration
                                               , averageTransitionDuration: TimeSpan.FromMinutes(20)
                                               , dueTime: currentTime + TimeSpan.FromDays(365)
                                               , customerDue: currentTime + TimeSpan.FromDays(365)
                                               , currentTime: currentTime

                    ),
                    ActorRefs.NoSender,
                    currentTime,
                    bucketSize));

            return jobConfirmation.ToImmutable() as JobConfirmationRecord;

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