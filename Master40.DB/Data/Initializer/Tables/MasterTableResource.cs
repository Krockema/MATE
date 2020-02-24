using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableResource
    {
        internal M_Resource SAW_1;
        internal M_Resource SAW_2;
        internal M_Resource SAW_3;
        internal M_Resource SAW_4;
        internal M_Resource SAW_5;
        internal M_Resource SAW_6;
        internal M_Resource DRILL_1;
        internal M_Resource DRILL_2;
        internal M_Resource DRILL_3;
        internal M_Resource ASSEMBLY_1;
        internal M_Resource ASSEMBLY_2;
        internal M_Resource ASSEMBLY_3;
        internal M_Resource ASSEMBLY_4;
        internal M_Resource ASSEMBLY_5;
        internal M_Resource ASSEMBLY_6;

        internal MasterTableResource()
        {
            SAW_1 = new M_Resource { Name = "Saw 1", Count = 1 };
            SAW_2 = new M_Resource { Name = "Saw 2", Count = 1 };
            SAW_3 = new M_Resource { Name = "Saw 3", Count = 1 };
            SAW_4 = new M_Resource { Name = "Saw 4", Count = 1 };
            SAW_5 = new M_Resource { Name = "Saw 5", Count = 1 };
            SAW_6 = new M_Resource { Name = "Saw 6", Count = 1 };
            DRILL_1 = new M_Resource { Name = "Drill 1", Count = 1 };
            DRILL_2 = new M_Resource { Name = "Drill 2", Count = 1 };
            DRILL_3 = new M_Resource { Name = "Drill 3", Count = 1 };
            ASSEMBLY_1 = new M_Resource { Name = "Assembly Unit 1", Count = 1 };
            ASSEMBLY_2 = new M_Resource { Name = "Assembly Unit 2", Count = 1 };
            ASSEMBLY_3 = new M_Resource { Name = "Assembly Unit 3", Count = 1 };
            ASSEMBLY_4 = new M_Resource { Name = "Assembly Unit 4", Count = 1 };
            ASSEMBLY_5 = new M_Resource { Name = "Assembly Unit 5", Count = 1 };
            ASSEMBLY_6 = new M_Resource { Name = "Assembly Unit 6", Count = 1 };
        }

        internal M_Resource[] InitMedium(MasterDBContext context)
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

        internal M_Resource[] InitLarge(MasterDBContext context)
        {
            var resources = new M_Resource[] {
                SAW_1,
                SAW_2,
                SAW_3,
                SAW_4,
                SAW_5,
                SAW_6,
                DRILL_1,
                DRILL_2,
                DRILL_3,
                ASSEMBLY_1,
                ASSEMBLY_2,
                ASSEMBLY_3,
                ASSEMBLY_4,
                ASSEMBLY_5,
                ASSEMBLY_6
            };
            context.Resources.AddRange(entities: resources);
            context.SaveChanges();
            return resources;
        }
    }
}