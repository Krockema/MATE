using System.Threading.Tasks;
using Master40.DB.DB.Models;

namespace Master40.Simulation.Simulation
{
    public interface ISimulationItem
    {
        int Start { get; set; }
        int End { get; set; }
        SimulationState SimulationState { get; set; }
        Task<bool> DoAtStart();
        Task<bool> DoAtEnd();
    }
}