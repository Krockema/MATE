module FProductionResults

open System
open IKeys

    type public FProductionResult =
        { Key : Guid
          ArticleKey : Guid
          CreationTime : int64
          Amount : decimal
        } interface IKey with
                member this.Key  with get() = this.Key
                member this.CreationTime with get() = this.CreationTime
