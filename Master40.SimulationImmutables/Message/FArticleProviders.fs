module FArticleProviders

open Akka.Actor
open System

    type public FArticleProvider =
        {
            ArticleKey : Guid 
            ArticleName : string
            StockExchangeId : Guid
            ArticleFinishedAt : int64
            Provider : System.Collections.Generic.List<Guid>
        }
