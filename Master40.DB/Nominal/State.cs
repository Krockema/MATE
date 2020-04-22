namespace Master40.DB.Nominal
{
    public enum State
    {
        Created,
        Injected,
        ProviderExist,
        BackwardScheduleExists,
        ForwardScheduleExists,
        ExistsInCapacityPlan,
        Producing,
        Finished,
        InProgress
    }

    public enum JobState
    {
        Created,
        ToDissolve,
        InQueue,
        Requeue,
        Ready,
        InProcess,
        InSetup,
        Finish
    }

    public enum ProducingState
    {
        Created,
        Waiting,
        Producing,
        Finished
    }
}
