namespace Master40.DB.DB.Models
{
    public class MachineTool : BaseEntity
    {
        public int MachineId { get; set; }
        public string Name { get; set; }
        public Machine Machine { get; set; }
        public int SetupTime { get; set; }
    }
}
