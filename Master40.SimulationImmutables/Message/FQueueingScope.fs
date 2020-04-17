module FQueueingScopes

open IScopes
open FSetupScopes
open FProcessingScopes
open System.Linq

    type public FQueueingScope = 
        {   IsQueueAble : bool
            IsRequieringSetup : bool
            Scopes : List<IScope>
         } 
            member this.GetScopeStart() = this.Scopes.Min(fun y -> y.Start)
            member this.GetScopeEnd() = this.Scopes.Max(fun y -> y.End)
            member this.GetSetup() = this.Scopes.SingleOrDefault(fun y -> y.GetType().Equals(typeof<FSetupScope>))
            member this.GetProcessing() = this.Scopes.SingleOrDefault(fun y -> y.GetType().Equals(typeof<FProcessingScope>))