using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.GeneratorModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DataGenerator.Repository
{
    public class ApproachRepository
    {

        public static Approach GetApproachById(DataGeneratorContext ctx, int id)
        {
            var approach = ctx.Approaches
                .Include(x => x.BomInput)
                .Include(x => x.ProductStructureInput)
                .Include(x => x.TransitionMatrixInput)
                    .ThenInclude(x => x.WorkingStations)
                        .ThenInclude(x => x.MachiningTimeParameterSet)
                .Include(x => x.TransitionMatrixInput)
                    .ThenInclude(x => x.GeneralMachiningTimeParameterSet)
                .Single(predicate: (x => x.Id == id));
            return approach;
        }

    }
}