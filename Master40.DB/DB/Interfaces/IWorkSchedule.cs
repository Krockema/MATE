
namespace Master40.DB.DB.Interfaces
{
    public interface IWorkSchedule
    {
        int HierarchyNumber { get; set; }
        string Name { get; set; }
        int Duration { get; set; }
        int MachineGroupId { get; set; }
    }
}
