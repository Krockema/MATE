module ITimeRanges

type public ITimeRange = 
    abstract member Start : int64 with get
    abstract member End : int64 with get