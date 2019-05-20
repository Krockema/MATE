
namespace Master40.DB.Interfaces
{
    public interface IOperation
    {
        int HierarchyNumber { get; set; }
        string Name { get; set; }
        int Duration { get; set; }
        int MachineGroupId { get; set; }
    }
}
