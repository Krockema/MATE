using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSimulate;
using NSimulate.Instruction;
using NSimulate.Example4;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Make a simulation context
            using (var context = new SimulationContext(isDefaultContextForProcess: true))
            {
                // instantiate the process responsible for setting alarms
                new AlarmSettingProcess();

                new SleepingProcess();

                // instantate a new simulator
                var simulator = new Simulator();

                // run the simulation
                simulator.Simulate();

                Console.WriteLine("Simulation ended at time period {0}", context.TimePeriod);
            }
        }
    }
}
