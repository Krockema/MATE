module FArticleProviders

open Akka.Actor
open System
open FStockProviders

    type public FArticleProvider =
        {
            ArticleKey : Guid 
            ArticleName : string
            StockExchangeId : Guid
            ArticleFinishedAt : int64
            CustomerDue : int64
            Provider : System.Collections.Generic.List<FStockProvider>
        }
