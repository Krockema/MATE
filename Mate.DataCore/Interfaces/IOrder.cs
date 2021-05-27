using Mate.DataCore.Nominal;

namespace Mate.DataCore.Interfaces
{
    public interface IOrder
    {
        int BusinessPartnerId { get; set; }
        int CreationTime { get; set; }
        int DueTime { get; set; }
        int FinishingTime { get; set; }
        string Name { get; set; }
        State State { get; set; }
    }
}