module FArticles

open System
open Master40.DB.DataModel
open Akka.Actor
open IKeys
open FStockProviders

    type public FArticle =         
        {   Key : Guid
            CreationTime : int64
            Article : M_Article
            StockExchangeId : Guid
            StorageAgent: IActorRef
            Quantity : int
            DueTime : int64
            CustomerDue : int64
            RemainingDuration : int64
            OriginRequester : IActorRef
            DispoRequester : IActorRef
            ProviderList : System.Collections.Generic.List<FStockProvider> 
            CustomerOrderId : int
            IsProvided : bool
            ProvidedAt : int64
            IsHeadDemand : bool 
            FinishedAt : int64 } 
            interface IKey with
                member this.Key with get() = this.Key
                member this.CreationTime with get() = this.CreationTime
            member this.UpdateFinishedAt f = { this with FinishedAt = f }
            member this.UpdateProvidedAt t = { this with ProvidedAt = t }
            member this.UpdateOriginRequester r = { this with OriginRequester = r }
            member this.UpdateDispoRequester r = { this with DispoRequester = r }
            member this.UpdateCustomerOrderAndDue id due storage = { this with CustomerOrderId = id; DueTime = due; StorageAgent = storage }
            member this.UpdateArticle article = { this with Article = article }
            member this.UpdateStorageAgent s = { this with StorageAgent = s }
            member this.UpdateStockExchangeId i = { this with StockExchangeId  = i }
            member this.UpdateProviderList p = { this with ProviderList = p }
            member this.SetProvided = { this with IsProvided = true }

