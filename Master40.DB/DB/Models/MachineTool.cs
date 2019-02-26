using Newtonsoft.Json;
using System.Collections.Generic;

namespace Master40.DB.Models
{
    public class MachineTool : BaseEntity
    {
        public const string MACHINE_FKEY = "Machine";
        public int MachineId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public Machine Machine { get; set; }

        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; }
        public int SetupTime { get; set; }
        public string Discription { get; set; }
    }
}
