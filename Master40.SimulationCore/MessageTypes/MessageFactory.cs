using Master40.SimulationImmutables;
using System;
using Master40.FunctionConverter;
using Master40.DB.Models;
using Akka.Actor;
using System.Collections.Generic;

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
        public static WorkItem ToWorkItem(this WorkSchedule workSchedule, long dueTime, ElementStatus status, IActorRef productionAgent, long time)
        {
            var prioRule = Extension.CreateFunc(
                // Lamda zur Func.
                (currentTime) => dueTime - workSchedule.Duration - currentTime
                // ENDE
                );

            return new WorkItem(key: Guid.NewGuid()
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
                                , proposals: new List<Proposal>());
        }

        public static RequestItem ToRequestItem(this OrderPart orderPart, IActorRef requester)
        {
            return new RequestItem(
                key: Guid.NewGuid()
                , dueTime: orderPart.Order.DueTime
                , quantity: orderPart.Quantity
                , article: orderPart.Article
                , orderId: orderPart.Id
                , isHeadDemand: true
                , stockExchangeId: Guid.Empty
                , storageAgent: ActorRefs.NoSender
                , requester: requester
                , providable: 0
                , provided: false
                , providerList: new List<IActorRef>()
            );
        }
    }


}
