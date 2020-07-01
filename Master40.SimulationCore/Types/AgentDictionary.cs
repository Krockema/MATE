using System;
using Akka.Actor;
using System.Collections.Generic;
using System.Linq;

namespace Master40.SimulationCore.Types
{
    public class AgentDictionary : Dictionary<object, IActorRef>
    {
        public List<IActorRef> ToSimpleList()
        {
            return this.Select(x => x.Value).ToList();
        }
    }

}
