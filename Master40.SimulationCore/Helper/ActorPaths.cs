﻿using Akka.Actor;
using Master40.SimulationCore.Agents;
using System;
using System.Collections.Generic;
using Master40.SimulationCore.Agents.Guardian;

namespace Master40.SimulationCore.Helper
{
    public class ActorPaths
    {
        public ActorMetaData SystemAgent { get; private set; }
        public ActorMetaData StorageDirectory { get; private set; }
        public ActorMetaData HubDirectory { get; private set; }
        public ActorMetaData SimulationContext { get; }
        public IActorRef SystemMailBox { get; }
        public Dictionary<GuardianType, IActorRef> Guardians { get; }
        /// <summary>
        /// Static helper class used to define paths to fixed-name actors
        /// (helps eliminate errors when using <see cref="ActorSelection"/>)
        /// </summary>
        public ActorPaths(IActorRef simulationContext, IActorRef systemMailBox)
        {
            SimulationContext = new ActorMetaData(name: "SimulationContext", actorRef: simulationContext);
            SystemMailBox = systemMailBox;
            Guardians = new Dictionary<GuardianType, IActorRef>();
        }

        public void SetSupervisorAgent(IActorRef systemAgent)
        {
            SystemAgent = new ActorMetaData(name: "SupervisorAgent", actorRef: systemAgent);
        }

        public void SetHubDirectoryAgent(IActorRef hubAgent)
        {
            if (SystemAgent == null) throw new Exception(message: "Wrong Order, Please SetSystemAgent first");
            HubDirectory = new ActorMetaData(name: "HubDirectory", actorRef: hubAgent);
        }

        public void SetStorageDirectory(IActorRef storageAgent)
        {
            if (SystemAgent == null) throw new Exception(message: "Wrong Order, Please SetSystemAgent first");
            StorageDirectory = new ActorMetaData(name: "StorageDirectory", actorRef: storageAgent);
        }

        public void AddGuardian(GuardianType guardianType, IActorRef actorRef)
        {
            Guardians.Add(key: guardianType, value: actorRef);
        }

    }
}
