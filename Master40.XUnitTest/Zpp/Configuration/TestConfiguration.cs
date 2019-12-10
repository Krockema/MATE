using Zpp.LotSize;

namespace Master40.XUnitTest.Zpp.Configuration
{
    public class TestConfiguration
    {
        public int CustomerOrderPartQuantity { get; set; }
        public int LotSize { get; set; }
        
        public LotSizeType LotSizeType { get; set; }
        public string Name { get; set; }
        
        // classname as full AssemblyQualifiedName see MSDoc AssemblyQualifiedName
        public string DbSetInitializer { get; set; }
        
        // classname as full AssemblyQualifiedName see MSDoc AssemblyQualifiedName
        public string TestScenario { get; set; }
    }
}