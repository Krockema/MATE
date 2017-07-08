using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class MachineTool : BaseEntity
    {
        public int MachineId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public Machine Machine { get; set; }
        public int SetupTime { get; set; }
    }
}
