module FQueueingPositions

type public FQueueingPosition = {
    IsQueueAble : bool
    IsRequieringSetup : bool
    Start : int64
    End : int64
    EstimatedWork : int64 
}