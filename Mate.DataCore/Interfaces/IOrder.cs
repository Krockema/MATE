using Mate.DataCore.Nominal;
using System;

namespace Mate.DataCore.Interfaces
{
    public interface IOrder
    {
        int BusinessPartnerId { get; set; }
        DateTime CreationTime { get; set; }
        DateTime DueTime { get; set; }
        DateTime FinishingTime { get; set; }
        string Name { get; set; }
        State State { get; set; }
    }
}