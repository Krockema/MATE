module FThroughPutTimes


open System

type public FThroughPutTime =
       { ArticleKey : Guid
         ArticleName : string
         Start : int64
         End : int64
       } 