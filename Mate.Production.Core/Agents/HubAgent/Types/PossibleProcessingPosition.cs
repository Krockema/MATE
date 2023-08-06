using System;
using System.Collections.Generic;
using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.Production.Core.Environment.Records.Scopes;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    public class PossibleProcessingPosition
    {
        public M_ResourceCapabilityProvider ResourceCapabilityProvider { get; }
        public DateTime _processingPosition { get; set; }
        public Dictionary<IActorRef, ScopeConfirmationRecord> _queuingDictionary { get; set; } = new Dictionary<IActorRef, ScopeConfirmationRecord>();

        public bool RequireSetup { get; set; } = false;

        public PossibleProcessingPosition(M_ResourceCapabilityProvider resourceCapabilityProvider)
        {
            ResourceCapabilityProvider = resourceCapabilityProvider;

        }

        public void Add(IActorRef resourceRef, ScopeConfirmationRecord fScopeConfirmation)
        {
            _queuingDictionary.Add(resourceRef, fScopeConfirmation);
        }

        public void Add(IActorRef resourceRef, ScopeConfirmationRecord fScopeConfirmation, DateTime processingStart)
        {
            _processingPosition = processingStart;
            _queuingDictionary.Add(resourceRef, fScopeConfirmation);
        }

    }
}
