module FUpdateStartConditions

open System

type public FUpdateStartCondition =
       { OperationKey : Guid
         PreCondition : bool
         ArticlesProvided : bool
         } 