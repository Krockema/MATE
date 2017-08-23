using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Master40.Agents;
using Master40.Agents.Agents;
using Master40.Agents.Agents.Internal;
using Master40.Agents.Agents.Model;
using Master40.DB.Data.Context;
using Master40.MessageSystem.Messages;
using Master40.MessageSystem.SignalR;

namespace Master40.BusinessLogicCentral.MRP
{
    public class AgentSimulator
    {
        private readonly ProductionDomainContext _context;
        private AgentSimulation _agentSimulation;
        private IMessageHub _messageHub;
        public AgentSimulator(ProductionDomainContext context, IMessageHub messageHub)
        {
            _context = context;
            _agentSimulation = new AgentSimulation(context, messageHub);
            _messageHub = messageHub;
        }

        public async Task RunSimulation(int simulationId)
        {
            await _agentSimulation.RunSim(simulationId);
            _messageHub.SendToAllClients("Number of involved Agents: " + Agent.AgentCounter.Count, MessageType.success);
            _messageHub.SendToAllClients("Number of instructions send: " + Agent.InstructionCounter, MessageType.success);


            var itemlist = from val in Agent.AgentStatistics
                group val by new { val.AgentType } into grouped
                select new { Agent = grouped.First().AgentType, ProcessingTime = grouped.Sum(x => x.ProcessingTime), Count = grouped.Count().ToString() };

            foreach (var item in itemlist)
            {
                _messageHub.SendToAllClients(" Agent (" + Agent.AgentCounter.Count(x => x == item.Agent) + "): " + item.Agent + " -> Runtime: " + item.ProcessingTime + " Milliseconds with " + item.Count + " Instructions Processed", MessageType.warning);
            }

            //var jobs = itemlist.Count(x => x.)
            var jobs = AgentStatistic.Log.Count(x => x.Contains("Finished Work with"));
            _messageHub.SendToAllClients(jobs + " Jobs processed in " + (Agent.AgentStatistics.Max(x => x.Time) - 1) + " minutes", MessageType.success);
            _messageHub.SendToAllClients("Simulation Finished");
            _messageHub.EndScheduler();
        }


    }
}