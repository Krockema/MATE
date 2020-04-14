using Akka.Actor;
using Master40.DB.DataModel;
using System.Collections.Generic;
using static FQueueingPositions;

namespace Master40.SimulationCore.Agents.HubAgent.Types
{
    public class PossibleProcessingPosition
    {
        public M_ResourceCapabilityProvider _resourceCapabilityProvider { get; set; }
        public long _processingPosition { get; set; }
        public Dictionary<IActorRef, FQueueingPosition> _queuingDictionary { get; set; }

        public PossibleProcessingPosition(M_ResourceCapabilityProvider resourceCapabilityProvider)
        {
            _resourceCapabilityProvider = resourceCapabilityProvider;

        }

        public void Add(IActorRef resourceRef, FQueueingPosition fQueueingPosition)
        {
            _queuingDictionary.Add(resourceRef, fQueueingPosition);
        }

        public void Add(IActorRef resourceRef, FQueueingPosition fQueueingPosition, long processingStart)
        {
            _processingPosition = processingStart;
            _queuingDictionary.Add(resourceRef, fQueueingPosition);
        }

    }
}
