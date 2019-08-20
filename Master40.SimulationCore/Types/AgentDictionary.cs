using System;
using Akka.Actor;
using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Master40.SimulationCore.Types
{
    public class AgentDictionary : Dictionary<IActorRef, object>
    {
        public List<IActorRef> ToSimpleList()
        {
            var actors = new List<IActorRef>();
            foreach (var item in this)
            {
                actors.Add(item: item.Key);
            }
            return actors;
        }
    }

}
