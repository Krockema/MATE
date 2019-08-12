using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.SimulationCore.Helper
{
    public class AgentDictionary : Dictionary<IActorRef, object>
    {
        public List<IActorRef> ToSimpleList()
        {
            var actors = new List<IActorRef>();
            foreach (var item in this)
            {
                actors.Add(item.Key);
            }
            return actors;
        }
    }

}
