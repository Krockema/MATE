using System.Threading.Tasks;

namespace Master40.Simulation.Simulation
{
    public interface ISimulationItem
    {
        int Start { get; set; }
        int End { get; set; }
        Task<bool> DoAtStart();
        Task<bool> DoAtEnd();
    }
}