using System;
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

namespace Master40.SimulationCore.Helper
{
    public static class MessageFactory
    {
        /// <summary>
        /// TODO: Fulfill Creator
        /// </summary>
        /// <param name="dueTime"></param>
        /// <param name="prio"></param>
        /// <returns></returns>
        public static FOperation ToOperationItem(this M_Operation operation
                                            , long dueTime
                                            , IActorRef productionAgent
                                            , bool lastLeaf 
                                            , long time)
        {
            var prioRule = Extension.CreateFunc(
                    // Lamda zur Func.
                    (currentTime) => dueTime - operation.Duration - currentTime
                    // ENDE
                );

            return new FOperation(key: Guid.NewGuid()
                                , dueTime: dueTime
                                , creationTime: time
                                , forwardStart: 0
                                , forwardEnd: 0
                                , backwardStart: 0
                                , backwardEnd: 0
                                , end: 0
                                , start: 0
                                , startConditions: new FStartCondition(preCondition: lastLeaf, materialsProvided: false)
                                , priority: prioRule.ToFSharpFunc()
                                , resourceAgent: ActorRefs.NoSender
                                , hubAgent: ActorRefs.NoSender
                                , productionAgent: productionAgent
                                , operation: operation
                                , proposals: new List<FProposal>());
        }

        public static FBucket ToBucketItem(this FOperation operation, long time)
        {
            // TO BE TESTET
            var prioRule = Extension.CreateFunc(
                    // Lamda zur Func.
                    (bucket, currentTime) => bucket.Operations.Min(y => ((IJob)y).Priority(time))
                    // ENDE
                );
            var operations = new List<FOperation>();
            operations.Add(operation);

            return new FBucket(key: Guid.NewGuid()
                                //, prioRule: prioRule.ToFSharpFunc()
                                , priority: prioRule.ToFSharpFunc()
                                , creationTime: time
                                , forwardStart: 0
                                , forwardEnd: 0
                                , backwardStart: 0
                                , backwardEnd: 0
                                , end: 0
                                , start: 0
                                , startConditions: new FStartCondition(preCondition: false, materialsProvided: false)
                                , maxBucketSize: 1
                                , minBucketSize: 1
                                , resourceAgent: ActorRefs.NoSender
                                , hubAgent: ActorRefs.NoSender
                                , operations: new FSharpSet<FOperation>(operations)
                                , proposals: new List<FProposal>());
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
                , customerOrderId: orderPart.Id
                , isHeadDemand: true
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , isProvided: false
                , originRequester: requester
                , dispoRequester: ActorRefs.Nobody
                , providerList: new List<IActorRef>()
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
                , quantity: Convert.ToInt32(articleBom.Quantity)
                , article: articleBom.ArticleChild
                , customerOrderId: requestItem.CustomerOrderId
                , isHeadDemand: false
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , originRequester: requester
                , dispoRequester: ActorRefs.Nobody
                , providerList: new List<IActorRef>()
                , finishedAt: 0
            );
        }
    }


}
