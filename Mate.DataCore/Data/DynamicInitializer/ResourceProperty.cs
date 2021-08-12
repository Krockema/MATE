namespace Mate.DataCore.Data.DynamicInitializer
{
    public class ResourceProperty
    {
        public string Name { get; set; }
        public int ToolCount { get; set; }
        public int ResourceCount { get; set; }
        public int SetupTime { get; set; }
        public int OperatorCount { get; set; }
        public bool IsBatchAble { get; set;}
        public double BatchSize { get; set; }
    }
}