module FCreateStabilityMeasurements

open System

    type public FCreateStabilityMeasurement = {
        Keys : ResizeArray<string>
        Resource : string
        Time : int64
        Position : int
        Start : int64
        Process : string
    }
