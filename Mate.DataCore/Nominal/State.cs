namespace Mate.DataCore.Nominal
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
        Revoked,
        RevokeStarted,
        Created,
        InQueue,
        WillBeReady,
        SetupReady,
        SetupInProcess,
        SetupFinished,
        Ready,
        InProcess,
        Finish,
    }

    public enum ProducingState
    {
        Created,
        Waiting,
        Producing,
        Finished
    }

    public enum GanttConfirmationState
    {
        Started = 1,
        Finished = 16
    }

    public enum GanttActivityState
    {
        Unplanned = 1,
        PartialPlanned = 2,
        Planned = 4,
        Started = 8,
        PartialConfirmed = 16,
        Finished = 32
    }

    public enum GanttStockPostingType
    {
        Relatively = 0, // change value by withdraw or insert material to stock
        Absolutely = 1  // set new value, i.e. reset stock
    }

}
