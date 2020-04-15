using Akka.Actor;
using Master40.DB.DataModel;
using System.Collections.Generic;
using static FQueueingScopes;
using static FScopeConfirmations;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class PossibleProcessingPosition
    {
        public M_ResourceCapabilityProvider _resourceCapabilityProvider { get; set; }
        public long _processingPosition { get; set; }
        public Dictionary<IActorRef, FScopeConfirmation> _queuingDictionary { get; set; } = new Dictionary<IActorRef, FScopeConfirmation>();

        public PossibleProcessingPosition(M_ResourceCapabilityProvider resourceCapabilityProvider)
        {
            _resourceCapabilityProvider = resourceCapabilityProvider;

        }

        public void Add(IActorRef resourceRef, FScopeConfirmation fScopeConfirmation)
        {
            _queuingDictionary.Add(resourceRef, fScopeConfirmation);
        }

        public void Add(IActorRef resourceRef, FScopeConfirmation fScopeConfirmation, long processingStart)
        {
            _processingPosition = processingStart;
            _queuingDictionary.Add(resourceRef, fScopeConfirmation);
        }

    }
}
