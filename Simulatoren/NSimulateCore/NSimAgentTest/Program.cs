using System;
using System.Collections.Generic;
using NSimAgentTest.Agents;
using NSimAgentTest.Agents.Internal;
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
                var system = CreateModel(context: context, numberOfJobs: 500);

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
            var ctx = new DBContext();
            var system = new SystemAgent(null, "System", true);



            // Create Directory Agent,
            var directoryAgent = new DirectoryAgent(system, "Directory", true);
            system.ChildAgents.Add(directoryAgent);

            // Create Machine Agents
            foreach (var machine in ctx.Machines)
            {
                system.ChildAgents.Add(new MachineAgent(creator: system, 
                                                           name: "Machine: " + machine, 
                                                          debug: true, 
                                                 directoryAgent: directoryAgent,
                                                    machineType: machine)); 
            }

            // Create Stock Agents
            foreach (var stock in ctx.StockElements)
            {
                system.ChildAgents.Add(new StorageAgent(creator: system, 
                                                           name: stock.Name, 
                                                          debug: true, 
                                                   stockElement: stock ));
            }

            // Create Contract agents, has to be done by system, during the simulation
            foreach (var contract in ctx.OrderList)
            {
                var ca = new ContractAgent(creator: system, 
                                             name: contract.Name, 
                                            debug: true);

                // enqueue Order
                ca.InstructionQueue.Enqueue(new InstructionSet
                {
                   MethodName = ContractAgent.InstuctionsMethods.StartOrder.ToString(),
                   ObjectToProcess = contract,
                   ObjectType = typeof(RequestItem),
                   SourceAgent = system // maybe Later System
                });

                // add To System
                system.ChildAgents.Add(ca);
            }
            
            // Return System Agent to Context
            return system;
        }
    }
}