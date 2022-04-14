using System;

namespace Mate.Production.Core.Agents.CollectorAgent.Types
{
    public class ArSimElement
    {
        public string stuffID { get; private set; }
        public int location { get; private set; }
        public string targetMachine { get; private set; }
        public long duration { get; private set; }
        public int transportRatio { get; private set; }


        public ArSimElement(string stuffId, string targetMachineId, long processDuration, int transportDurRatio)
        {
            stuffID = stuffId;
            location = -1;
            targetMachine = targetMachineId;
            duration = processDuration;
            transportRatio = transportDurRatio;
        }

    }
}
