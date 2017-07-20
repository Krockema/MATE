using System;
using NSimulate;

namespace NSimAgentTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Simulation Startet");
            using (var context = new SimulationContext(isDefaultContextForProcess: true))
            {
                // initialise the model
                var machines = CreateModel(context: context, numberOfJobs: 500);

                // instantate a new simulator
                var simulator = new Simulator();

                // run the simulation
                simulator.Simulate();

                Console.WriteLine("Jobs processed in {0} minutes", context.TimePeriod);
            }
            Console.ReadLine();
        }

        private static object CreateModel(SimulationContext context, int numberOfJobs)
        {
            throw new NotImplementedException();
        }
    }
}