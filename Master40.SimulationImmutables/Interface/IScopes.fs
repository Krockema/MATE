module IScopes

type public IScope = 
    abstract member Start : int64 with get
    abstract member End : int64 with get
    abstract member EstimatedWork : int64 with get