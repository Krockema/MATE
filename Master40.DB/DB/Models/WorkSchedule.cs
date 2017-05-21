using Master40.DB.Models.Interfaces;

namespace Master40.DB.Models
{
    public class WorkSchedule : BaseEntity, IWorkSchedule
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int MachineGroupId { get; set; }
        public MachineGroup MachineGroup { get; set; }
        public int ArticleId { get; set; }
        public Article Article { get; set; }
    }
}