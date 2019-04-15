using Master40.DB.Interfaces;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Operation : BaseEntity, IWorkSchedule
    {
        public int HierarchyNumber { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public int MachineGroupId { get; set; }
        [JsonIgnore]
        public M_MachineGroup MachineGroup { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int MachineToolId { get; set; }
        public M_MachineTool MachineTool { get; set; }
    }
}