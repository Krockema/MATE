module FScopeConfirmations

open IScopes
open System.Linq
open FSetupScopes
open FProcessingScopes

    type public FScopeConfirmation =         
        {   
            Scopes : List<IScope>
        } 
            member this.GetScopeStart() = this.Scopes.Min(fun y -> y.Start)
            member this.GetScopeEnd() = this.Scopes.Max(fun y -> y.End)
            member this.GetSetup() = this.Scopes.SingleOrDefault(fun y -> y.GetType().Equals(typeof<FSetupScope>))
            member this.GetProcessing() = this.Scopes.SingleOrDefault(fun y -> y.GetType().Equals(typeof<FProcessingScope>))