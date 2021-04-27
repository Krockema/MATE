using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Master40.FunctionConverter;
using Mate.DataCore.DataModel;
using Mate.DataCore.GanttPlan;
using Mate.DataCore.GanttPlan.GanttPlanModel;
using Mate.DataCore.Nominal;
using Mate.DataCore.ReportingModel;
using Mate.Production.Core.Types;
using Microsoft.FSharp.Collections;
using static FArticles;
using static FBuckets;
using static FCreateSimulationJobs;
using static FMeasurementInformations;
using static FOperations;
using static FStartConditions;

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
        public static FOperation ToOperationItem(this M_Operation m_operation
                                            , PriorityRule priorityRule
                                            , long dueTime
                                            , long customerDue
                                            , IActorRef productionAgent
                                            , bool firstOperation
                                            , long currentTime
                                            , long remainingWork
                                            , Guid articleKey)
        {

            IDictionary<PriorityRule, Func<long, double>> priorityFunctions = new Dictionary<PriorityRule, Func<long, double>>
            {
                { PriorityRule.LST, (time) => (customerDue - time) - m_operation.Duration - remainingWork },
                { PriorityRule.MDD, (time) => (new double[] { dueTime, time + remainingWork + m_operation.Duration}).Max() },
                { PriorityRule.FIFO, (time) => currentTime },
                { PriorityRule.SPT, (time) => m_operation.Duration }
            };

            return new FOperation(key: Guid.NewGuid()
                                , dueTime: dueTime
                                , customerDue: customerDue
                                , creationTime: currentTime
                                , forwardStart: currentTime
                                , forwardEnd: currentTime + m_operation.Duration + m_operation.AverageTransitionDuration
                                , backwardStart: dueTime - m_operation.Duration - m_operation.AverageTransitionDuration
                                , backwardEnd: dueTime
                                , remainingWork: remainingWork
                                , end: 0
                                , start: 0
                                , startConditions: new FStartCondition(preCondition: firstOperation, articlesProvided: false, 0)
                                , priority: priorityFunctions[priorityRule].ToFSharpFunc()
                                , setupKey: -1 // unset
                                , isFinished: false
                                , hubAgent: ActorRefs.NoSender
                                , productionAgent: productionAgent
                                , operation: m_operation
                                , requiredCapability: m_operation.ResourceCapability
                                , bucket: String.Empty
                                , articleKey: articleKey);
        }

        public static FBucket ToBucketScopeItem(this FOperation operation, IActorRef hubAgent, long time, long maxBucketSize)
        {
            //Todo Abs of BackwardStart and ForwardStart, avoid negative values
            //var scope = Math.Abs(operation.BackwardStart - operation.ForwardStart);
            var scope = (operation.BackwardStart - operation.ForwardStart);
            // TO BE TESTET
            var prioRule = Extension.CreateFunc(
                // Lamda zur Func.
                func: (bucket, currentTime) => bucket.Operations.Min(selector: y => ((IJobs.IJob)y).Priority(currentTime))
                // ENDE
            );

            var operations = new List<FOperation> {operation};

            return new FBucket(key: Guid.NewGuid()
                //, prioRule: prioRule.ToFSharpFunc()
                , priority: prioRule.ToFSharpFunc()
                , name: $"(Bucket({BucketNumber++})){operation.RequiredCapability.Name}"
                , isFixPlanned: false
                , creationTime: time
                , forwardStart: operation.ForwardStart
                , forwardEnd: operation.ForwardEnd
                , backwardStart: operation.BackwardStart
                , backwardEnd: operation.BackwardEnd
                , scope: scope
                , end: 0
                , start: 0
                , startConditions: new FStartConditions.FStartCondition(preCondition: false, articlesProvided: false, 0)
                , maxBucketSize: maxBucketSize
                , minBucketSize: 1000
                , setupKey: -1 //unset
                , hubAgent: hubAgent
                , operations: new FSharpSet<FOperation>(elements: operations)
                , requiredCapability: operation.RequiredCapability
                , bucket: String.Empty);
        }


        public static FArticle ToRequestItem(this T_CustomerOrderPart orderPart
                                            , IActorRef requester
                                            , long customerDue
                                            , long remainingDuration
                                            , long currentTime)
        {
            var article = new FArticles.FArticle(
                key: Guid.Empty
                , keys: new FSharpSet<Guid>(new Guid[] { })
                , dueTime: orderPart.CustomerOrder.DueTime
                , quantity: orderPart.Quantity
                , article: orderPart.Article
                , creationTime: currentTime
                , customerOrderId: orderPart.CustomerOrderId
                , isHeadDemand: true
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , isProvided: false
                , customerDue: customerDue
                , remainingDuration : remainingDuration
                , providedAt: 0
                , originRequester: requester
                , dispoRequester: ActorRefs.Nobody
                , providerList: new List<FStockProviders.FStockProvider>()
                , finishedAt: 0
            );
            return article.CreateProductionKeys.SetPrimaryKey;
        }

        public static FArticle ToRequestItem(this M_ArticleBom articleBom
                                                , FArticle requestItem
                                                , IActorRef requester
                                                , long customerDue
                                                , long remainingDuration
                                                , long currentTime)
        {
            var article = new FArticles.FArticle(
                key: Guid.Empty
                , keys: new FSharpSet<Guid>(new Guid[] { })
                , dueTime: requestItem.DueTime
                , creationTime: currentTime
                , isProvided: false
                , quantity: Convert.ToInt32(value: articleBom.Quantity)
                , article: articleBom.ArticleChild
                , customerOrderId: requestItem.CustomerOrderId
                , isHeadDemand: false
                , providedAt: 0
                , customerDue: customerDue
                , remainingDuration: remainingDuration
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , originRequester: requester
                , dispoRequester: ActorRefs.Nobody
                , providerList: new List<FStockProviders.FStockProvider>(100)
                , finishedAt: 0
            );
            return article.CreateProductionKeys.SetPrimaryKey;
        }

        public static FCreateSimulationJob ToSimulationJob(this FOperation fOperation, string jobType, FArticle fArticle, string productionAgent)
        {
            var simulationJob = new FCreateSimulationJob(
                key: fOperation.Key.ToString()
                , dueTime: fOperation.DueTime
                , articleName: fOperation.Operation.Article.Name
                , operationName: fOperation.Operation.Name
                , operationHierarchyNumber: fOperation.Operation.HierarchyNumber
                , operationDuration: fOperation.Operation.Duration
                , requiredCapabilityName: fOperation.Operation.ResourceCapability.Name
                , jobType: jobType.ToString()
                , customerOrderId: fArticle.CustomerOrderId.ToString()
                , isHeadDemand: fArticle.IsHeadDemand
                , fArticleKey: fArticle.Key
                , fArticleName: fArticle.Article.Name
                , productionAgent: productionAgent
                , articleType: fArticle.Article.ArticleType.Name
                , jobName: fOperation.Bucket
                , capabilityProvider: string.Empty
                , start: fOperation.Start
                , end: fOperation.End
            );

            return simulationJob;
        }

        public static FCreateSimulationJob ToSimulationJob(this GptblProductionorderOperationActivity activity, long start, long duration, string requiredCapabilityName)
        {
            var simulationJob = new FCreateSimulationJob(
                key: activity.GetKey
                , dueTime: activity.InfoDateLatestEndMaterial.Value.ToSimulationTime()
                , articleName: activity.Productionorder.MaterialId
                , operationName: activity.Name
                , operationHierarchyNumber: Int32.Parse(activity.OperationId)
                , operationDuration: duration
                , requiredCapabilityName: requiredCapabilityName
                , jobType: activity.ActivityId.Equals(2) ? JobType.SETUP : JobType.OPERATION
                , customerOrderId: string.Empty
                , isHeadDemand: false
                , fArticleKey: Guid.Empty
                , fArticleName: string.Empty
                , productionAgent: string.Empty
                , articleType: string.Empty
                , jobName: activity.Name
                , capabilityProvider: string.Empty
                , start: start
                , end: start + duration
            );

            return simulationJob;
        }

        public static SimulationMeasurement CreateMeasurement(FOperation job, M_Characteristic characteristic, M_Attribute attribute, FMeasurementInformation fMeasurementInformation)
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
