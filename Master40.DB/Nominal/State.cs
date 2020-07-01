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

    public enum ProducingState
    {
        Created,
        Waiting,
        Producing,
        Finished
    }
}
