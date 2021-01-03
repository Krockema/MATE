using Microsoft.ML.Data;

namespace AiProvider.DataStuctures
{
    public class SimulationKpis
    {
        //[ColumnName("Time"), LoadColumn(0)]
        public float Time { get; set; }

        //[ColumnName("Lateness"), LoadColumn(1)]
        public float Lateness { get; set; }

        //[ColumnName("Assembly"), LoadColumn(2)]
        public float Assembly { get; set; }

        //[ColumnName("Total"), LoadColumn(3)]
        public float Total { get; set; }

        //[ColumnName("CycleTime"), LoadColumn(4)]
        public float CycleTime { get; set; }

        //[ColumnName("Consumab"), LoadColumn(5)]
        public float Consumab { get; set; }

        //[ColumnName("Material"), LoadColumn(6)]
        public float Material { get; set; }

        //[ColumnName("InDueTotal"), LoadColumn(7)]
        public float InDueTotal { get; set; }

    }
}
