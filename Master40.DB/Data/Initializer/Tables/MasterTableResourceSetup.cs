using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public class MasterTableResourceSetup
    {
        internal M_ResourceSetup SAW_1_BLADE_BIG;
        internal M_ResourceSetup SAW_2_BLADE_BIG;
        internal M_ResourceSetup SAW_2_BLADE_SMALL;
        internal M_ResourceSetup DRILL_1_M4;
        internal M_ResourceSetup DRILL_1_M6;
        internal M_ResourceSetup ASSEMBLY_1_SCREWDRIVER;
        internal M_ResourceSetup ASSEMBLY_2_SCREWDRIVER;

        internal MasterTableResourceSetup(MasterTableResource resource
                                        , MasterTableResourceTool resourceTool
                                        , MasterTableResourceSkill resourceSkill)
        {
            SAW_1_BLADE_BIG = new M_ResourceSetup
            {
                Name = resource.SAW_1.Name + " blade big",
                ResourceId = resource.SAW_1.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                SetupTime = 5
            };

            SAW_2_BLADE_BIG = new M_ResourceSetup
            {
                Name = resource.SAW_2.Name + " blade big",
                ResourceId = resource.SAW_2.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                SetupTime = 5
            };

            SAW_2_BLADE_SMALL = new M_ResourceSetup
            {
                Name = resource.SAW_2.Name + " blade small",
                ResourceId = resource.SAW_2.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                SetupTime = 5
            };

            DRILL_1_M4 = new M_ResourceSetup
            {
                Name = resource.DRILL_1.Name + " M4",
                ResourceId = resource.DRILL_1.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M4.Id,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                SetupTime = 10
            };

            DRILL_1_M6 = new M_ResourceSetup
            {
                Name = resource.DRILL_1.Name + " M6",
                ResourceId = resource.DRILL_1.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_1_SCREWDRIVER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_1.Name + " screwdriver universal",
                ResourceId = resource.ASSEMBLY_1.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 5
            };

            ASSEMBLY_2_SCREWDRIVER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_2.Name + " screwdriver universal",
                ResourceId = resource.ASSEMBLY_2.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 5
            };
        }

        public M_ResourceSetup[] Init(MasterDBContext context)
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