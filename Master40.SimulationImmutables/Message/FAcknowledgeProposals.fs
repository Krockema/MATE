module FAcknowledgeProposals

open Akka.Actor
open IJobs

type public FAcknowledgeProposal = {
    Job : IJob
    Schedule : int64
    Resources : System.Collections.Generic.List<IActorRef>
}