using System;
using System.Diagnostics;
using System.Linq;
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

                Debug.WriteLine("Number of involved Agents: " +  Agent.AgentCounter.Count);
                Debug.WriteLine("Number of instructions send: " + Agent.InstructionCounter);


                var itemlist = from val in Agent.AgentStatistics
                    group val by new { val.Agent } into grouped
                    select new { Agent = grouped.First().Agent, ProcessingTime = grouped.Sum(x => x.ProcessingTime), Count = grouped.Count().ToString()};

                foreach (var item in itemlist)
                {
                    Debug.WriteLine(" Agent (" + Agent.AgentCounter.Count(x => x == item.Agent) +"): " +item.Agent + " -> Runtime: " +item.ProcessingTime +" Milliseconds with " + item.Count + " Instructions Processed");
                }
                foreach (var machine in context.ActiveProcesses.Where(x => x.GetType() == typeof(MachineAgent)))
                {
                    var item = ((MachineAgent) machine);
                    Debug.WriteLine("Agent " + item.Name + " Queue Length:" + item.Queue.Count);
                }


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
            foreach (var machine in _productionDomainContext.Machines.Include(m => m.MachineGroup))
            {
                system.ChildAgents.Add(new MachineAgent(creator: system, 
                                                           name: "Machine: " + machine.Name, 
                                                          debug: true, 
                                                 directoryAgent: directoryAgent,
                                                        machine: machine)); 
            }

            // Create Stock Agents
            foreach (var stock in _productionDomainContext.Stocks.Include(x => x.StockExchanges).Include(x => x.Article)
                                                                                                .ThenInclude(x => x.ArticleToBusinessPartners))
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