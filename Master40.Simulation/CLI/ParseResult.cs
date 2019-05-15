using System;
using System.Collections.Generic;
using System.Text;
using Akka.Pattern;
using Master40.DB.Enums;

namespace Master40.Simulation.CLI
{
    public class ParseResult
    {
        public int ConfigId { get; set; } = 1;

        public SimulationType SimulationType { get; set; } = SimulationType.Central;
    }
}
