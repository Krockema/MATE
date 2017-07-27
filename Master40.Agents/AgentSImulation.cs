using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Master40.Agents.Agents;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Data.Context;
using Microsoft.EntityFrameworkCore;
using NSimulate;

namespace Master40.Agents
{
    public class AgentSimulation 
    {
        private readonly ProductionDomainContext _productionDomainContext;

        public AgentSimulation(ProductionDomainContext _productionDomainContext)
        {
            this._productionDomainContext = _productionDomainContext;
        }

        public async Task RunSim()
        {
            Debug.WriteLine("Simulation Startet");
            using (var context = new SimulationContext(isDefaultContextForProcess: true))
            {
                // initialise the model
                var system = CreateModel(context: context, numberOfJobs: 500);

                // instantate a new simulator
                var simulator = new Simulator();

                // run the simulation
                await simulator.SimulateAsync(0);

               Debug.WriteLine("Jobs processed in {0} minutes", context.TimePeriod);
            }
        }

        private object CreateModel(SimulationContext context, int numberOfJobs)
        {
            var system = new SystemAgent(null, "System", true, _productionDomainContext);





            // Create Directory Agent,
            var directoryAgent = new DirectoryAgent(system, "Directory", true);
            system.ChildAgents.Add(directoryAgent);

            // Create Machine Agents
            foreach (var machine in _productionDomainContext.Machines)
            {
                system.ChildAgents.Add(new MachineAgent(creator: system, 
                                                           name: "Machine: " + machine, 
                                                          debug: true, 
                                                 directoryAgent: directoryAgent,
                                                    machineType: machine)); 
            }

            // Create Stock Agents
            foreach (var stock in _productionDomainContext.Stocks.Include(x => x.Article).Include(x => x.StockExchanges))
            {
                system.ChildAgents.Add(new StorageAgent(creator: system, 
                                                           name: stock.Name, 
                                                          debug: true, 
                                                   stockElement: stock ));
            }

            system.PrepareAgents();
            
            
            // Return System Agent to Context
            return system;
        }
    }
}