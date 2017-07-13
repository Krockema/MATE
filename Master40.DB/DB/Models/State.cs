namespace Master40.DB.DB.Models
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
}
