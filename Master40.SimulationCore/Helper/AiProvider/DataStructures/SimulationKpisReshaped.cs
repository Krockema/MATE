using Microsoft.ML.Data;

namespace AiProvider.DataStuctures
{
    public class SimulationKpisReshaped
    {
        public SimulationKpisReshaped(float time, float lateness = 0, float assembly = 0, float total = 0, float cycleTime = 0, float consumable = 0, float material = 0, float inDueTotal = 0)
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
        public float Lateness_t2 { get; set; }

        public float Assembly_t2 { get; set; }
        public float Total_t2 { get; set; }
        public float CycleTime_t2 { get; set; }

        public float Consumab_t2 { get; set; }
        public float Material_t2 { get; set; }

        public float InDueTotal_t2 { get; set; }
        public float Lateness_t1 { get; set; }

        public float Assembly_t1 { get; set; }

        public float Total_t1 { get; set; }
        public float CycleTime_t1 { get; set; }

        public float Consumab_t1 { get; set; }

        public float Material_t1 { get; set; }

        public float InDueTotal_t1 { get; set; }
        public float Lateness_t0 { get; set; }

        public float Assembly_t0 { get; set; }
        public float Total_t0 { get; set; }
        public float Consumab_t0 { get; set; }

        public float Material_t0 { get; set; }
        public float InDueTotal_t0 { get; set; }
        public float CycleTime_t0 { get; set; }
    }
}
