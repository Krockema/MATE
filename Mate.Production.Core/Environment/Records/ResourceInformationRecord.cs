using Akka.Actor;
using Mate.DataCore.DataModel;
using Mate.DataCore.Nominal.Model;
using Mate.Production.Core.Helper.DistributionProvider;
using System.Collections.Immutable;

namespace Mate.Production.Core.Environment.Records
{
    public record ResourceInformationRecord(
        int ResourceId,
        string ResourceName,
        IImmutableSet<M_ResourceCapabilityProvider> ResourceCapabilityProvider,
        ResourceType ResourceType,
        WorkTimeGenerator WorkTimeGenerator,
        string RequiredFor,
        IActorRef Ref)
    {
        public ResourceInformationRecord UpdateRef(IActorRef r) => this with { Ref = r };
    };
}

