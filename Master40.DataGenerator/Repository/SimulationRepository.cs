using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.GeneratorModel;

namespace Master40.DataGenerator.Repository
{
    public class SimulationRepository
    {
        public static Simulation GetSimulationById(int id, DataGeneratorContext ctx)
        {
            return ctx.Simulations.SingleOrDefault(x => x.Id == id);
        }
    }
}