using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.MessageTypes;
using Master40.SimulationImmutables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Agents.ProductionAgent.Behaviour
{
    public static class BehaviourFactory
    {
        public static IBehaviour Get(SimulationType simType)
        {
            switch (simType)
            {
                case SimulationType.Bucket:
                    return Bucket();
                default:
                    return Default();
            }


        }

        private static IBehaviour Default()
        {
            var properties = new Dictionary<string, object>();

            //properties.Add(REQUEST_ITEM, new object()); // RequestItem
            properties.Add(Production.Properties.WORK_ITEMS, new List<FWorkItem>());
            properties.Add(Production.Properties.REQUESTED_ITEMS, new List<FRequestItem>());
            properties.Add(Production.Properties.HUB_AGENTS, new Dictionary<IActorRef, string>());
            properties.Add(Production.Properties.ELEMENT_STATUS, ElementStatus.Created);
            properties.Add(Production.Properties.NEXT_WORK_ITEM, new object());
            properties.Add(Production.Properties.CHILD_WORKITEMS, new Queue<FRequestItem>());

            return new Default(properties);
        }

        private static IBehaviour Bucket()
        {
            var properties = new Dictionary<string, object>();

            //properties.Add(REQUEST_ITEM, new object()); // RequestItem
            properties.Add(Production.Properties.WORK_ITEMS, new List<FWorkItem>());
            properties.Add(Production.Properties.REQUESTED_ITEMS, new List<FRequestItem>());
            properties.Add(Production.Properties.HUB_AGENTS, new Dictionary<IActorRef, string>());
            properties.Add(Production.Properties.ELEMENT_STATUS, ElementStatus.Created);
            properties.Add(Production.Properties.NEXT_WORK_ITEM, new object());
            properties.Add(Production.Properties.CHILD_WORKITEMS, new Queue<FRequestItem>());

            return new Bucket(properties);
        }
    }
}
