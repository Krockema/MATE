using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;
using Mate.Production.Core.Types;
using Akka.Hive.Definitions;
using IdentityModel;
using Mate.Production.Core.Environment.Records;
using MathNet.Numerics.Statistics;
using System.Collections.Immutable;
using Mate.Production.Core.Environment.Records.Reporting;

namespace Mate.Production.Core.Helper
{
    public static class MessageFactory
    {
        private static int BucketNumber = 0;
        /// <summary>
        /// Fulfill Creator
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="dueTime"></param>
        /// <param name="productionAgent"></param>
        /// <param name="firstOperation"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static OperationRecord ToOperationRecord(this M_Operation m_operation
                                            , PriorityRule priorityRule
                                            , DateTime dueTime
                                            , DateTime customerDue
                                            , IActorRef productionAgent
                                            , bool firstOperation
                                            , DateTime currentTime
                                            , TimeSpan remainingWork
                                            , Guid articleKey)
        {

            IDictionary<PriorityRule, Func<DateTime, double>> priorityFunctions = new Dictionary<PriorityRule, Func<DateTime, double>>
            {
                { PriorityRule.LST, (time) => ((customerDue - time) - m_operation.Duration - remainingWork).TotalSeconds },
                { PriorityRule.MDD, (time) => ((new [] { dueTime, time + remainingWork + m_operation.Duration}).Max()).ToEpochTime() },
                { PriorityRule.FIFO, (time) => currentTime.ToEpochTime() },
                { PriorityRule.SPT, (time) => m_operation.Duration.TotalSeconds }
            };

            return new OperationRecord(Key: Guid.NewGuid()
                                , DueTime: dueTime
                                , CustomerDue: customerDue
                                , CreationTime: currentTime
                                , ForwardStart: currentTime
                                , ForwardEnd: currentTime + m_operation.Duration + m_operation.AverageTransitionDuration
                                , BackwardStart: dueTime - m_operation.Duration - m_operation.AverageTransitionDuration
                                , BackwardEnd: dueTime
                                , RemainingWork: remainingWork
                                , End: Time.ZERO.Value
                                , Start: Time.ZERO.Value
                                , StartCondition: new StartConditionRecord(PreCondition: firstOperation, ArticlesProvided: false, Time.ZERO.Value)
                                , Priority: priorityFunctions[priorityRule]
                                , SetupKey: -1 // unset
                                , IsFinished: false
                                , HubAgent: ActorRefs.NoSender
                                , ProductionAgent: productionAgent
                                , Operation: m_operation
                                , RequiredCapability: m_operation.ResourceCapability
                                , Bucket: String.Empty
                                , ArticleKey: articleKey);
        }

        public static BucketRecord ToBucketScopeItem(this OperationRecord operation, IActorRef hubAgent, DateTime time, TimeSpan maxBucketSize)
        {
            //Todo Abs of BackwardStart and ForwardStart, avoid negative values
            //var scope = Math.Abs(operation.BackwardStart - operation.ForwardStart);
            var scope = (operation.BackwardStart - operation.ForwardStart);
            // TO BE TESTET
            Func<BucketRecord, DateTime, double> prioRule = (bucket, currentTime) => bucket.Operations.Min(selector: y => ((IJob)y).Priority(currentTime)
            // ENDE
            );

            var operations = ImmutableHashSet.Create(operation);

            return new BucketRecord(
                //, prioRule: prioRule.ToFSharpFunc()
                Name: $"(Bucket({BucketNumber++})){operation.RequiredCapability.Name}"
                , Priority: prioRule
                , IsFixPlanned: false
                , CreationTime: time
                , ForwardStart: operation.ForwardStart
                , ForwardEnd: operation.ForwardEnd
                , BackwardStart: operation.BackwardStart
                , BackwardEnd: operation.BackwardEnd
                , Scope: scope
                , End: Time.ZERO.Value
                , Start: Time.ZERO.Value
                , StartConditions: new StartConditionRecord(PreCondition: false, ArticlesProvided: false, Time.ZERO.Value)
                , MaxBucketSize: maxBucketSize
                , MinBucketSize: 1000
                , SetupKey: -1 //unset
                , HubAgent: hubAgent
                , Operations: operations
                , RequiredCapability: operation.RequiredCapability
                , Bucket: String.Empty);
        }


        public static ArticleRecord ToRequestItem(this T_CustomerOrderPart orderPart
                                            , IActorRef requester
                                            , DateTime customerDue
                                            , TimeSpan remainingDuration
                                            , DateTime currentTime)
        {
            var article = new ArticleRecord(
                Keys: ImmutableHashSet.Create<Guid>(new Guid[orderPart.Quantity])
                , DueTime: orderPart.CustomerOrder.DueTime
                , Quantity: orderPart.Quantity
                , Article: orderPart.Article
                , CreationTime: currentTime
                , CustomerOrderId: orderPart.CustomerOrderId
                , IsHeadDemand: true
                , StockExchangeId: Guid.Empty
                , StorageAgent: ActorRefs.NoSender
                , IsProvided: false
                , CustomerDue: customerDue
                , RemainingDuration: remainingDuration
                , ProvidedAt: Time.ZERO.Value
                , OriginRequester: requester
                , DispoRequester: ActorRefs.Nobody
                , ProviderList: ImmutableHashSet.Create<StockProviderRecord>()
                , FinishedAt: Time.ZERO.Value
            );
            return article;
        }

        public static ArticleRecord ToRequestItem(this M_ArticleBom articleBom
                                                , ArticleRecord requestItem
                                                , IActorRef requester
                                                , DateTime customerDue
                                                , TimeSpan remainingDuration
                                                , DateTime currentTime)
        {
            var article = new ArticleRecord(
                Keys: ImmutableHashSet.Create<Guid>(new Guid[requestItem.Quantity])
                , DueTime: requestItem.DueTime
                , CreationTime: currentTime
                , IsProvided: false
                , Quantity: Convert.ToInt32(value: articleBom.Quantity)
                , Article: articleBom.ArticleChild
                , CustomerOrderId: requestItem.CustomerOrderId
                , IsHeadDemand: false// requestItem.IsHeadDemand
                , ProvidedAt: Time.ZERO.Value
                , CustomerDue: customerDue
                , RemainingDuration: remainingDuration
                , StockExchangeId: Guid.Empty
                , StorageAgent: ActorRefs.NoSender
                , OriginRequester: requester
                , DispoRequester: ActorRefs.Nobody
                , ProviderList: ImmutableHashSet.Create<StockProviderRecord>()
                , FinishedAt: Time.ZERO.Value
            );
            return article;
        }

        public static CreateSimulationJobRecord ToSimulationJob(this OperationRecord fOperation, string jobType, ArticleRecord fArticle, string productionAgent)
        {
            var simulationJob = new CreateSimulationJobRecord(
                Key: fOperation.Key.ToString()
                , DueTime: fOperation.DueTime
                , ArticleName: fOperation.Operation.Article.Name
                , OperationName: fOperation.Operation.Name
                , OperationHierarchyNumber: fOperation.Operation.HierarchyNumber
                , OperationDuration: fOperation.Operation.Duration
                , RequiredCapabilityName: fOperation.Operation.ResourceCapability.Name
                , JobType: jobType.ToString()
                , CustomerOrderId: fArticle.CustomerOrderId.ToString()
                , IsHeadDemand: fArticle.IsHeadDemand
                , ArticleKey: fArticle.Key
                , ArticleNameRecord: fArticle.Article.Name
                , ProductionAgent: productionAgent
                , ArticleType: fArticle.Article.ArticleType.Name
                , JobName: fOperation.Bucket
                , CapabilityProvider: string.Empty
                , Start: fOperation.Start
                , End: fOperation.End
            );

            return simulationJob;
        }

        public static CreateSimulationJobRecord ToSimulationJob(this GptblProductionorderOperationActivity activity, DateTime start, TimeSpan duration, string requiredCapabilityName)
        {
            var simulationJob = new CreateSimulationJobRecord(
                Key: activity.GetKey
                , DueTime: activity.InfoDateLatestEndMaterial.Value
                , ArticleName: activity.Productionorder.MaterialId
                , OperationName: activity.Name
                , OperationHierarchyNumber: Int32.Parse(activity.OperationId)
                , OperationDuration: duration
                , RequiredCapabilityName: requiredCapabilityName
                , JobType: activity.ActivityId.Equals(2) ? JobType.SETUP : JobType.OPERATION
                , CustomerOrderId: string.Empty
                , IsHeadDemand: false
                , ArticleKey: Guid.Empty
                , ArticleNameRecord: string.Empty
                , ProductionAgent: string.Empty
                , ArticleType: string.Empty
                , JobName: activity.Name
                , CapabilityProvider: string.Empty
                , Start: start
                , End: start + duration
            );

            return simulationJob;
        }

        public static SimulationMeasurement CreateMeasurement(OperationRecord job, M_Characteristic characteristic, M_Attribute attribute, MeasurementInformationRecord fMeasurementInformation)
        {
            return new SimulationMeasurement
            {
                JobId = job.Key,
                ArticleKey = job.ArticleKey,
                JobName = job.Operation.Name,
                ArticleName = job.Operation.Article.Name,
                CharacteristicName = characteristic.Name,
                ResourceTool = fMeasurementInformation.Tool,
                Resource = fMeasurementInformation.Resource,
                TargetValue = attribute.Value,
                AttributeName = attribute.Name
            };
        }

    }


}
