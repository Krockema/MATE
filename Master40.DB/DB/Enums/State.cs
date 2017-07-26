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
        Produced,
        Delivered,
        Purchased,
    }

    public enum ProducingState
    {
        Created,
        Waiting,
        Producing,
        Finished
    }
}
