using Microsoft.ML.Data;

namespace AiProvider.DataStuctures
{
    public class SimulationKpis
    {
        public SimulationKpis(float time, float lateness = 0, float assembly = 0, float total = 0, float cycleTime = 0, float consumable = 0, float material = 0, float inDueTotal = 0)
        {
            Time = time;
            Lateness = lateness;
            Assembly = assembly;
            Total = total;
            CycleTime = cycleTime;
            Consumab = consumable;
            Material = material;
            InDueTotal = inDueTotal;
        }
        public float Time { get; set; }

        public float Lateness { get; set; }
        public float Assembly { get; set; }
        public float Total { get; set; }

        public float CycleTime { get; set; }
        public float Consumab { get; set; }

        public float Material { get; set; }
        public float InDueTotal { get; set; }
    }
}
