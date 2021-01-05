using Microsoft.ML.Data;

namespace AiProvider.DataStuctures
{
    public class SimulationKpis
    {
        public SimulationKpis(long time, double lateness = 0, double assembly = 0, double total = 0, double cycleTime = 0, double consumable = 0, double material = 0, double inDueTotal = 0)
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

        //[ColumnName("Time"), LoadColumn(0)]
        public long Time { get; set; }

        //[ColumnName("Lateness"), LoadColumn(1)]
        public double Lateness { get; set; }

        //[ColumnName("Assembly"), LoadColumn(2)]
        public double Assembly { get; set; }

        //[ColumnName("Total"), LoadColumn(3)]
        public double Total { get; set; }

        //[ColumnName("CycleTime"), LoadColumn(4)]
        public double CycleTime { get; set; }

        //[ColumnName("Consumab"), LoadColumn(5)]
        public double Consumab { get; set; }

        //[ColumnName("Material"), LoadColumn(6)]
        public double Material { get; set; }

        //[ColumnName("InDueTotal"), LoadColumn(7)]
        public double InDueTotal { get; set; }
    }
}
