module FAcknowledgeProposals

open Akka.Actor
open IJobs
open FSetupDefinitions

type public FAcknowledgeProposal = {
    Job : IJob
    SetupDefinition : FSetupDefinition
}