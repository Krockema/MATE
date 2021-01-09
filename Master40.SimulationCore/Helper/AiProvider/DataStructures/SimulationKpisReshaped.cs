using Master40.DB.GanttPlanModel;
using Microsoft.ML.Data;

namespace AiProvider.DataStuctures
{
    public class SimulationKpisReshaped
    {
        //public SimulationKpisReshaped(
        //    float Lateness_t2,
        //    float Assembly_t2,
        //    float Total_t2,
        //
        //)
        //{
        //}
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
    }
}
