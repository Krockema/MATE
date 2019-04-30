using Master40.SimulationImmutables;
using System;
using Master40.FunctionConverter;
using Akka.Actor;
using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.MessageTypes
{
    public static class MessageFactory
    {
        /// <summary>
        /// TODO: Fulfill Creator
        /// </summary>
        /// <param name="dueTime"></param>
        /// <param name="prio"></param>
        /// <returns></returns>
        public static FWorkItem ToWorkItem(this M_Operation workSchedule, long dueTime, ElementStatus status, IActorRef productionAgent, long time)
        {
            var prioRule = Extension.CreateFunc(
                // Lamda zur Func.
                (currentTime) => dueTime - workSchedule.Duration - currentTime
                // ENDE
                );

            return new FWorkItem(key: Guid.NewGuid()
                                , dueTime: dueTime
                                , estimatedStart: 0
                                , estimatedEnd: 0
                                , materialsProvided: false
                                , status: status
                                , prioRule: prioRule.ToFSharpFunc()
                                , itemPriority: prioRule(time)
                                , wasSetReady: false
                                , resourceAgent: ActorRefs.NoSender
                                , hubAgent: ActorRefs.NoSender
                                , productionAgent: productionAgent
                                , workSchedule: workSchedule
                                , proposals: new List<FProposal>());
        }

        public static FRequestItem ToRequestItem(this T_CustomerOrderPart orderPart, IActorRef requester)
        {
            return new FRequestItem(
                key: Guid.NewGuid()
                , dueTime: orderPart.CustomerOrder.DueTime
                , quantity: orderPart.Quantity
                , article: orderPart.Article
                , customerOrderId: orderPart.Id
                , isHeadDemand: true
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , originRequester: requester
                , dispoRequester: ActorRefs.Nobody
                , providable: 0
                , provided: false
                , providerList: new List<IActorRef>()
                , finishedAt: 0
            );
        }

        public static FRequestItem ToRequestItem(this M_ArticleBom articleBom, FRequestItem requestItem, IActorRef requester)
        {
            return new FRequestItem(
                key: Guid.NewGuid()
                , dueTime: requestItem.DueTime
                , quantity: Convert.ToInt32(articleBom.Quantity)
                , article: articleBom.ArticleChild
                , customerOrderId: requestItem.CustomerOrderId
                , isHeadDemand: false
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , originRequester: requester
                , dispoRequester: ActorRefs.Nobody
                , providable: 0
                , provided: false
                , providerList: new List<IActorRef>()
                , finishedAt: 0
            );
        }
    }


}
