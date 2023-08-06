using Mate.DataCore.DataModel;
using System;

namespace Mate.Production.Core.Agents.HubAgent.Types
{
    public class BucketSize
    {
        public M_ResourceCapability Capability { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Size { get; set; }
        public double Ratio { get; set; }
    }
}
