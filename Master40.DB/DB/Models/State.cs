namespace Master40.DB.Models
{
    public enum State
    {
        Created,
        ProviderExist,
        BackwardScheduleExists,
        ForwardScheduleExists,
        ExistsInCapacityPlan,
        Produced,
        Deliverd,
        Purchased,
    }
}
