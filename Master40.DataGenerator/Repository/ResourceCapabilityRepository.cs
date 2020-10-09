using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.Repository
{
    public class ResourceCapabilityRepository
    {

        public static List<M_ResourceCapability> GetParentResourceCapabilities(MasterDBContext ctx)
        {
            return ctx.ResourceCapabilities.Where(x => x.ParentResourceCapabilityId == null).ToList();
        }

    }
}