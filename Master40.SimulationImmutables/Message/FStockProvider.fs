module FStockProviders

open System

type public FStockProvider = {
    ProvidesArticleKey : Guid
    ProductionAgentKey : string
}