module FComputationalTimers


open System

type public FComputationalTimer =
       { 
         time : int64
         timertype : string
         duration : int64
       } 