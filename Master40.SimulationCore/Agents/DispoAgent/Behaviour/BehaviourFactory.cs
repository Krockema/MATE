using Akka.Actor;
using Master40.DB.Enums;
using Master40.SimulationCore.MessageTypes;
using System.Collections.Generic;


namespace Master40.SimulationCore.Agents.DispoAgent.Behaviour
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
            var properties = new Dictionary<string, object>
            {
                { Dispo.Properties.STORAGE_AGENT_REF, ActorRefs.Nobody }
                ,{ Dispo.Properties.QUANTITY_TO_PRODUCE, 0.0 }
                // ,{ Dispo.Properties.REQUEST_ITEM, null }
            };
            return new Default(properties);

        }

        private static IBehaviour Bucket()
        {
            var properties = new Dictionary<string, object>
            {
                { Dispo.Properties.STORAGE_AGENT_REF, ActorRefs.Nobody }
                ,{ Dispo.Properties.QUANTITY_TO_PRODUCE, 0.0 }
                // ,{ Dispo.Properties.REQUEST_ITEM, null }
            };
            return new Bucket(properties);

        }
    }
}
