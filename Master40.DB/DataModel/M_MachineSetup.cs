using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    /// <summary>
    /// Join Table to N:M between Machine and Tool
    /// </summary>
    public class M_MachineSetup : BaseEntity
    {
        public string Name { get; set; }
        public int MachineId { get; set; }
        [JsonIgnore]
        public M_Machine Machine { get; set; }
        public int ToolId { get; set; }
        [JsonIgnore]
        public M_Tool Tool { get; set; }
        public int MachineGroupId { get; set; }
        [JsonIgnore]
        public M_MachineGroup MahcineGroup { get; set; }
        public int SetupTime { get; set; }
        public string Discription { get; set; }
    }
}
