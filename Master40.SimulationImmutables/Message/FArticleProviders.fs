module FArticleProviders

open Akka.Actor
open System

    type public FArticleProvider =
        {
            ArticleKey : Guid 
            ArticleName : string
            StockExchangeId : Guid
            Provider : System.Collections.Generic.List<Guid>
        }
