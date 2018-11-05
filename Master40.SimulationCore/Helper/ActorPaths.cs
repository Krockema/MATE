using Akka.Actor;
using System;

namespace Master40.SimulationCore.Helper
{
    public class ActorPaths 
    {
        public ActorMetaData SystemAgent { get; private set; }
        public ActorMetaData StorageDirectory { get; private set; }
        public ActorMetaData HubDirectory { get; private set; }
        public ActorMetaData SimulationContext { get; }
        /// <summary>
        /// Static helper class used to define paths to fixed-name actors
        /// (helps eliminate errors when using <see cref="ActorSelection"/>)
        /// </summary>
        public ActorPaths(IActorRef simulationContext)
        {
            SimulationContext = new ActorMetaData("SimulationContext", simulationContext);
        }

        public void SetSystemAgent(IActorRef systemAgent)
        {
            SystemAgent = new ActorMetaData("SystemAgent", systemAgent);
        }

        public void SetHubDirectoryAgent(IActorRef hubAgent)
        {
            if (SystemAgent == null) throw new Exception("Wrong Order, Please SetSystemAgent first");
            HubDirectory = new ActorMetaData("HubDirectory", hubAgent);
        }

        public void SetStorageDirectory(IActorRef storageAgent)
        {
            if (SystemAgent == null) throw new Exception("Wrong Order, Please SetSystemAgent first");
            StorageDirectory = new ActorMetaData("StorageDirectory", storageAgent);
        }

    }
}
