using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableResource
    {
        internal static M_Resource SAW_1 = new M_Resource { Name = "Saw 1", Count = 1 };
        internal static M_Resource SAW_2 = new M_Resource { Name = "Saw 2", Count = 1 };
        internal static M_Resource DRILL_1 = new M_Resource { Name = "Drill 1", Count = 1 };
        internal static M_Resource ASSEMBLY_1 = new M_Resource { Name = "Assembly Unit 1", Count = 1 };
        internal static M_Resource ASSEMBLY_2 = new M_Resource { Name = "Assembly Unit 2", Count = 1 } ;
        internal static M_Resource[] Init(MasterDBContext context)
        {
            var resources = new M_Resource[] {
                SAW_1,
                SAW_2,
                DRILL_1,
                ASSEMBLY_1,
                ASSEMBLY_2
            };
            context.Resources.AddRange(entities: resources);
            context.SaveChanges();
            return resources;
        }
    }
}