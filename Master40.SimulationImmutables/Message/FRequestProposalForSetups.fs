module FRequestProposalForSetups

open IJobs

type public FRequestProposalForSetup = {
    Job : IJob
    SetupId : int32
}