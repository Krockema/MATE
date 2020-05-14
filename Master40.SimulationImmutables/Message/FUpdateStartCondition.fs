module FUpdateStartConditions

open System

type public FUpdateStartCondition =
       { OperationKey : Guid
         CustomerDue : int64
         PreCondition : bool
         ArticlesProvided : bool
       } 