module FRequestProposalForCapabilityProviders

open IJobs

type public FRequestProposalForCapabilityProvider = {
    Job : IJob
    CapabilityProviderId : int32
}