module FComputationalTimers


open System
open Akka.Hive.Definitions

type public FComputationalTimer =
       { 
         time : Time
         timertype : string
         duration : TimeSpan
       } 