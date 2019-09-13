using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public static class MasterTableResourceSetup
    {
        internal static M_ResourceSetup SAW_1_BLADE_BIG = new M_ResourceSetup
        {
            Name = MasterTableResource.SAW_1.Name + " blade big", ResourceId = MasterTableResource.SAW_1.Id,
            ResourceToolId = MasterTableResourceTool.SAW_BLADE_BIG.Id,
            ResourceSkillId = MasterTableResourceSkill.CUTTING.Id, SetupTime = 5
        };

        internal static M_ResourceSetup SAW_2_BLADE_BIG = new M_ResourceSetup
        {
            Name = MasterTableResource.SAW_2.Name + " blade big", ResourceId = MasterTableResource.SAW_2.Id,
            ResourceToolId = MasterTableResourceTool.SAW_BLADE_BIG.Id,
            ResourceSkillId = MasterTableResourceSkill.CUTTING.Id, SetupTime = 5
        };

        internal static M_ResourceSetup SAW_2_BLADE_SMALL = new M_ResourceSetup
        {
            Name = MasterTableResource.SAW_2.Name + " blade small", ResourceId = MasterTableResource.SAW_2.Id,
            ResourceToolId = MasterTableResourceTool.SAW_BLADE_SMALL.Id,
            ResourceSkillId = MasterTableResourceSkill.CUTTING.Id, SetupTime = 5
        };

        internal static M_ResourceSetup DRILL_1_M4 = new M_ResourceSetup
        {
            Name = MasterTableResource.DRILL_1.Name + " M4", ResourceId = MasterTableResource.DRILL_1.Id,
            ResourceToolId = MasterTableResourceTool.DRILL_HEAD_M4.Id,
            ResourceSkillId = MasterTableResourceSkill.DRILLING.Id, SetupTime = 10
        };

        internal static M_ResourceSetup DRILL_1_M6 = new M_ResourceSetup
        {
            Name = MasterTableResource.DRILL_1.Name + " M6", ResourceId = MasterTableResource.DRILL_1.Id,
            ResourceToolId = MasterTableResourceTool.DRILL_HEAD_M6.Id,
            ResourceSkillId = MasterTableResourceSkill.DRILLING.Id, SetupTime = 10
        };

        internal static M_ResourceSetup ASSEMBLY_1_SCREWDRIVER = new M_ResourceSetup
        {
            Name = MasterTableResource.ASSEMBLY_1.Name + " screwdriver universal",
            ResourceId = MasterTableResource.ASSEMBLY_1.Id,
            ResourceToolId = MasterTableResourceTool.ASSEMBLY_SCREWDRIVER.Id,
            ResourceSkillId = MasterTableResourceSkill.ASSEMBLING.Id, SetupTime = 5
        };

        internal static M_ResourceSetup ASSEMBLY_2_SCREWDRIVER = new M_ResourceSetup
        {
            Name = MasterTableResource.ASSEMBLY_2.Name + " screwdriver universal",
            ResourceId = MasterTableResource.ASSEMBLY_2.Id,
            ResourceToolId = MasterTableResourceTool.ASSEMBLY_SCREWDRIVER.Id,
            ResourceSkillId = MasterTableResourceSkill.ASSEMBLING.Id, SetupTime = 5
        };
        public static M_ResourceSetup[] Init(MasterDBContext context)
        {
            var resourceSetups = new M_ResourceSetup[]{
                SAW_1_BLADE_BIG,
                SAW_2_BLADE_BIG,
                SAW_2_BLADE_SMALL,
                DRILL_1_M4,
                DRILL_1_M6,
                ASSEMBLY_1_SCREWDRIVER,
                ASSEMBLY_2_SCREWDRIVER
            };
            context.ResourceSetups.AddRange(entities: resourceSetups);
            context.SaveChanges();
            return resourceSetups;
        }

    }
}