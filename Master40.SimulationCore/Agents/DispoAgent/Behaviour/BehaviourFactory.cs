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

            return new Default();

        }

        private static IBehaviour Bucket()
        {

            return new Bucket();

        }
    }
}
