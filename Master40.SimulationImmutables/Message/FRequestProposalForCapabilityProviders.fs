module FRequestProposalForCapabilityProviders

open IJobs

type public FRequestProposalForCapability = {
    Job : IJob
    CapabilityId : int32
}