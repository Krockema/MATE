using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master40.Simulation.Simulation
{
    public class PowsSimulationItem : ISimulationItem
    {
        public int Start { get; set; }
        public int End { get; set; }
        public Task<bool> DoAtStart()
        {
            return null;
        }

        public Task<bool> DoAtEnd()
        {
            return null;
        }
    }
}
