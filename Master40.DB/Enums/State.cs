namespace Master40.DB.Enums
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
        Finished
    }

    public enum ProducingState
    {
        Created,
        Waiting,
        Producing,
        Finished
    }
}
