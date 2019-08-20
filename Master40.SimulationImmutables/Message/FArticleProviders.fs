module FArticleProviders

open Akka.Actor
open System

    type public FArticleProvider =
        {
            ArticleKey : Guid 
            ArticleName : string
            Provider : System.Collections.Generic.List<Guid>
        }
