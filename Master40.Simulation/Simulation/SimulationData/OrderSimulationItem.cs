using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Master40.Simulation.Simulation.SimulationData;

namespace Master40.Simulation.Simulation
{
    public class OrderSimulationItem : ISimulationItem
    {
        public OrderSimulationItem(int start, int end, ProductionDomainContext context, List<int> articleIds, List<int> amounts, int duetime, int simulationConfigurationId)
        {
            SimulationState = SimulationState.Waiting;
            Start = start;
            End = end;
            _context = context;
            ArticleIds = articleIds;
            Amounts = amounts;
            DueTime = duetime;
            SimulationConfigurationId = simulationConfigurationId;
        }
        public int DueTime { get; set; }
        public List<int> ArticleIds { get; set; }
        public List<int> Amounts { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        private ProductionDomainContext _context { get; set; }
        public SimulationState SimulationState { get; set; }
        public int SimulationConfigurationId { get; set; }
        public Task<bool> DoAtStart()
        {
            return null;
        }

        public Task<bool> DoAtEnd<T>(List<TimeTable<T>.MachineStatus> listMachineStatus) where T : ISimulationItem
        {
            for (var i = 0; i < ArticleIds.Count; i++)
            {
                _context.CreateNewOrder(ArticleIds[i], Amounts[i], _context.SimulationConfigurations.Single(a => a.Id == SimulationConfigurationId).Time, DueTime);
            }
          
            return null;
        }
    }
}
