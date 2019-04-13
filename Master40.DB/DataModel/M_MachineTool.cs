using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_MachineTool : BaseEntity
    {
        public const string MACHINE_FKEY = "Machine";
        public int MachineId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public M_Machine Machine { get; set; }

        public virtual ICollection<M_Operation> WorkSchedules { get; set; }
        public int SetupTime { get; set; }
        public string Discription { get; set; }
    }
}
