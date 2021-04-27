module FCentralStockPostings

open Akka.Actor

type public FCentralStockPosting = 
    {
        MaterialId : string
        Quantity : double
    }