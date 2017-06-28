using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Master40.Simulation.Simulation;

namespace Master40.BusinessLogic.Simulation
{
    public class TimeTable<T> where T : ISimulationItem
    {
        public TimeTable(int recalculateTimer = -1)
        {
            this.Timer = 0;
            this.RecalculateTimer = recalculateTimer;
            this.Items = new List<T>();
        }

        public List<T> Items { get; set; }

        public int Timer { get; set; }
        public int RecalculateTimer { get; set; }
        
    }
}
