using Master40.DB.Enums;
using Master40.DB.Nominal;

namespace Master40.DB.Interfaces
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