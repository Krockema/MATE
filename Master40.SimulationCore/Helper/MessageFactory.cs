﻿using System;
using Master40.FunctionConverter;
using Akka.Actor;
using System.Collections.Generic;
using Master40.DB.DataModel;
using Microsoft.FSharp.Collections;
using System.Linq;
using static FArticles;
using static FOperations;
using static FProposals;
using static FStartConditions;
using static FBuckets;
using static IJobs;
using Master40.SimulationCore.Types;
using static FStockProviders;
using static FSetupDefinitions;

namespace Master40.SimulationCore.Helper
{
    public static class MessageFactory
    {
        private static int BucketNumber = 0;
        /// <summary>
        /// Fulfill Creator
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="dueTime"></param>
        /// <param name="productionAgent"></param>
        /// <param name="firstOperation"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static FOperation ToOperationItem(this M_Operation operation
                                            , long dueTime
                                            , IActorRef productionAgent
                                            , bool firstOperation
                                            , long currentTime)
        {
            var prioRule = Extension.CreateFunc(
                    // Lamda zur Func.
                    func: (time) => dueTime - operation.Duration - time
                    // ENDE
                );

            return new FOperation(key: Guid.NewGuid()
                                , dueTime: dueTime
                                , creationTime: currentTime
                                , forwardStart: currentTime
                                , forwardEnd: currentTime + operation.Duration + operation.AverageTransitionDuration
                                , backwardStart: dueTime - operation.Duration - operation.AverageTransitionDuration
                                , backwardEnd: dueTime
                                , end: 0
                                , start: 0
                                , startConditions: new FStartCondition(preCondition: firstOperation, articlesProvided: false)
                                , priority: prioRule.ToFSharpFunc()
                                , setupKey: -1 // unset
                                , isFinished: false
                                , hubAgent: ActorRefs.NoSender
                                , productionAgent: productionAgent
                                , operation: operation
                                , requiredCapability: operation.ResourceCapability
                                , bucket: String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="hubAgent"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static FBucket ToBucketItem(this FOperation operation, IActorRef hubAgent, long time)
        {
            // TO BE TESTET
            var prioRule = Extension.CreateFunc(
                    // Lamda zur Func.
                    func: (bucket, currentTime) => bucket.Operations.Min(selector: y => ((IJob)y).Priority(time))
                    // ENDE
                );

            var operations = new List<FOperation>();
            operations.Add(item: operation);

            return new FBucket(key: Guid.NewGuid()
                                //, prioRule: prioRule.ToFSharpFunc()
                                , priority: prioRule.ToFSharpFunc()
                                , name: $"(Bucket({BucketNumber++})){operation.Operation.ResourceCapability.Name}"
                                , isFixPlanned: false
                                , creationTime: time
                                , forwardStart: operation.ForwardStart
                                , forwardEnd: operation.ForwardEnd
                                , backwardStart: operation.BackwardStart
                                , backwardEnd: operation.BackwardEnd
                                , scope: operation.BackwardStart - operation.ForwardStart
                                , end: 0
                                , start: 0
                                , startConditions: new FStartCondition(preCondition: false, articlesProvided: false)
                                , maxBucketSize: 1
                                , minBucketSize: 1000
                                , setupKey: -1
                                , hubAgent: hubAgent
                                , operations: new FSharpSet<FOperation>(elements: operations)
                                , requiredCapability: operation.RequiredCapability
                                , bucket: String.Empty);
        }

        public static FBucket ToBucketScopeItem(this FOperation operation, IActorRef hubAgent, long time)
        {
            //scope
            var scope = (operation.BackwardStart - operation.ForwardStart);
            // TO BE TESTET
            var prioRule = Extension.CreateFunc(
                // Lamda zur Func.
                func: (bucket, currentTime) => operation.BackwardEnd - scope - time
                // ENDE
            );

            var operations = new List<FOperation>();
            operations.Add(item: operation);

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
                , startConditions: new FStartCondition(preCondition: false, articlesProvided: false)
                , maxBucketSize: 1
                , minBucketSize: 1000
                , setupKey: -1 //unset
                , hubAgent: hubAgent
                , operations: new FSharpSet<FOperation>(elements: operations)
                , requiredCapability: operation.RequiredCapability
                , bucket: String.Empty);
        }


        public static FArticle ToRequestItem(this T_CustomerOrderPart orderPart
                                            , IActorRef requester
                                            , long currentTime)
        {
            return new FArticle(
                key: Guid.NewGuid()
                , dueTime: orderPart.CustomerOrder.DueTime
                , quantity: orderPart.Quantity
                , article: orderPart.Article
                , creationTime: currentTime
                , customerOrderId: orderPart.CustomerOrderId
                , isHeadDemand: true
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , isProvided: false
                , providedAt: 0
                , originRequester: requester
                , dispoRequester: ActorRefs.Nobody
                , providerList: new List<FStockProvider>()
                , finishedAt: 0
            );
        }

        public static FArticle ToRequestItem(this M_ArticleBom articleBom, FArticle requestItem, IActorRef requester, long currentTime)
        {
            return new FArticle(
                key: Guid.NewGuid()
                , dueTime: requestItem.DueTime
                , creationTime: currentTime
                , isProvided: false
                , quantity: Convert.ToInt32(value: articleBom.Quantity)
                , article: articleBom.ArticleChild
                , customerOrderId: requestItem.CustomerOrderId
                , isHeadDemand: false
                , providedAt: 0
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , originRequester: requester
                , dispoRequester: ActorRefs.Nobody
                , providerList: new List<FStockProvider>()
                , finishedAt: 0
            );
        }
    }


}
