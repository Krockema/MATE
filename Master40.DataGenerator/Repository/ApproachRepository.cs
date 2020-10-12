using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.GeneratorModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.DataGenerator.Repository
{
    public class ApproachRepository
    {

        public static InputParameter GetApproachById(DataGeneratorContext ctx, int id)
        {
            var approach = ctx.Approaches
                .Include(x => x.TransitionMatrix)
                .Single(predicate: (x => x.Id == id));
            return approach;
        }

    }
}