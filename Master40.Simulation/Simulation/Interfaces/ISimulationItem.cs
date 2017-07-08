using System.Collections.Generic;
using System.Threading.Tasks;
using Master40.DB.Models;
using Master40.Simulation.Simulation.SimulationData;

namespace Master40.Simulation.Simulation
{
    public interface ISimulationItem
    {
        int Start { get; set; }
        int End { get; set; }
        SimulationState SimulationState { get; set; }
        Task<bool> DoAtStart();
        //Task<bool> DoAtEnd(List<ISimulationItem>. );
        Task<bool> DoAtEnd<T>(List<TimeTable<T>.MachineStatus> listMachineStatus) where T : ISimulationItem;
    }
}