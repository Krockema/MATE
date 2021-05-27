using System.Collections.Generic;
using Akka.Actor;
using Mate.DataCore.DataModel;
using static FScopeConfirmations;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    public class PossibleProcessingPosition
    {
        public M_ResourceCapabilityProvider ResourceCapabilityProvider { get; }
        public long _processingPosition { get; set; }
        public Dictionary<IActorRef, FScopeConfirmation> _queuingDictionary { get; set; } = new Dictionary<IActorRef, FScopeConfirmation>();

        public bool RequireSetup { get; set; } = false;

        public PossibleProcessingPosition(M_ResourceCapabilityProvider resourceCapabilityProvider)
        {
            ResourceCapabilityProvider = resourceCapabilityProvider;

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
