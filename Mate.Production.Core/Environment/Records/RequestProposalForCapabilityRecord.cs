namespace Mate.Production.Core.Environment.Records
{
    public record RequestProposalForCapabilityRecord
    (
        IJob Job,
        int CapabilityId
    );
}

