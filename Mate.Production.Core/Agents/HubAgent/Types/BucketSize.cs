using Mate.DataCore.DataModel;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    public class BucketSize
    {
        public M_ResourceCapability Capability { get; set; }
        public long Duration { get; set; }
        public long Size { get; set; }
        public double Ratio { get; set; }
    }
}
