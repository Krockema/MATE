using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public class MasterTableResourceSetup
    {
        internal M_ResourceSetup SAW_1_BLADE_BIG;
        internal M_ResourceSetup SAW_1_BLADE_SMALL;
        internal M_ResourceSetup SAW_2_BLADE_BIG;
        internal M_ResourceSetup SAW_2_BLADE_SMALL;
        internal M_ResourceSetup SAW_3_BLADE_BIG;
        internal M_ResourceSetup SAW_3_BLADE_SMALL;
        internal M_ResourceSetup SAW_4_BLADE_BIG;
        internal M_ResourceSetup SAW_4_BLADE_SMALL;
        internal M_ResourceSetup SAW_5_BLADE_BIG;
        internal M_ResourceSetup SAW_5_BLADE_SMALL;
        internal M_ResourceSetup SAW_6_BLADE_BIG;
        internal M_ResourceSetup SAW_6_BLADE_SMALL;
        internal M_ResourceSetup DRILL_1_M4;
        internal M_ResourceSetup DRILL_1_M6;
        internal M_ResourceSetup DRILL_2_M4;
        internal M_ResourceSetup DRILL_2_M6;
        internal M_ResourceSetup DRILL_3_M4;
        internal M_ResourceSetup DRILL_3_M6;
        internal M_ResourceSetup ASSEMBLY_1_SCREWDRIVER;
        internal M_ResourceSetup ASSEMBLY_1_HOLDING;
        internal M_ResourceSetup ASSEMBLY_1_HAMMER;
        internal M_ResourceSetup ASSEMBLY_2_SCREWDRIVER;
        internal M_ResourceSetup ASSEMBLY_2_HOLDING;
        internal M_ResourceSetup ASSEMBLY_2_HAMMER;
        internal M_ResourceSetup ASSEMBLY_3_SCREWDRIVER;
        internal M_ResourceSetup ASSEMBLY_3_HOLDING;
        internal M_ResourceSetup ASSEMBLY_3_HAMMER;
        internal M_ResourceSetup ASSEMBLY_4_SCREWDRIVER;
        internal M_ResourceSetup ASSEMBLY_4_HOLDING;
        internal M_ResourceSetup ASSEMBLY_4_HAMMER;
        internal M_ResourceSetup ASSEMBLY_5_SCREWDRIVER;
        internal M_ResourceSetup ASSEMBLY_5_HOLDING;
        internal M_ResourceSetup ASSEMBLY_5_HAMMER;
        internal M_ResourceSetup ASSEMBLY_6_SCREWDRIVER;
        internal M_ResourceSetup ASSEMBLY_6_HOLDING;
        internal M_ResourceSetup ASSEMBLY_6_HAMMER;
        
        internal MasterTableResourceSetup(MasterTableResource resource
                                        , MasterTableResourceTool resourceTool
                                        , MasterTableResourceCapability resourceSkill)
        {
            SAW_1_BLADE_BIG = new M_ResourceSetup
            {
                Name = resource.SAW_1.Name + " blade big",
                ResourceId = resource.SAW_1.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_1_BLADE_SMALL = new M_ResourceSetup
            {
                Name = resource.SAW_1.Name + " blade small",
                ResourceId = resource.SAW_1.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_2_BLADE_BIG = new M_ResourceSetup
            {
                Name = resource.SAW_2.Name + " blade big",
                ResourceId = resource.SAW_2.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_2_BLADE_SMALL = new M_ResourceSetup
            {
                Name = resource.SAW_2.Name + " blade small",
                ResourceId = resource.SAW_2.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_3_BLADE_BIG = new M_ResourceSetup
            {
                Name = resource.SAW_3.Name + " blade big",
                ResourceId = resource.SAW_3.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_3_BLADE_SMALL = new M_ResourceSetup
            {
                Name = resource.SAW_3.Name + " blade small",
                ResourceId = resource.SAW_3.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_4_BLADE_BIG = new M_ResourceSetup
            {
                Name = resource.SAW_4.Name + " blade big",
                ResourceId = resource.SAW_4.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_4_BLADE_SMALL = new M_ResourceSetup
            {
                Name = resource.SAW_4.Name + " blade small",
                ResourceId = resource.SAW_4.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_5_BLADE_BIG = new M_ResourceSetup
            {
                Name = resource.SAW_5.Name + " blade big",
                ResourceId = resource.SAW_5.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_5_BLADE_SMALL = new M_ResourceSetup
            {
                Name = resource.SAW_5.Name + " blade small",
                ResourceId = resource.SAW_5.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_6_BLADE_BIG = new M_ResourceSetup
            {
                Name = resource.SAW_6.Name + " blade big",
                ResourceId = resource.SAW_6.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            SAW_6_BLADE_SMALL = new M_ResourceSetup
            {
                Name = resource.SAW_6.Name + " blade small",
                ResourceId = resource.SAW_6.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                ResourceCapabilityId = resourceSkill.CUTTING.Id,
                SetupTime = 20
            };

            DRILL_1_M4 = new M_ResourceSetup
            {
                Name = resource.DRILL_1.Name + " M4",
                ResourceId = resource.DRILL_1.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M4.Id,
                ResourceCapabilityId = resourceSkill.DRILLING.Id,
                SetupTime = 5
            };

            DRILL_1_M6 = new M_ResourceSetup
            {
                Name = resource.DRILL_1.Name + " M6",
                ResourceId = resource.DRILL_1.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                ResourceCapabilityId = resourceSkill.DRILLING.Id,
                SetupTime = 5
            };
            
            DRILL_2_M4 = new M_ResourceSetup
            {
                Name = resource.DRILL_2.Name + " M4",
                ResourceId = resource.DRILL_2.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M4.Id,
                ResourceCapabilityId = resourceSkill.DRILLING.Id,
                SetupTime = 5
            };

            DRILL_2_M6 = new M_ResourceSetup
            {
                Name = resource.DRILL_2.Name + " M6",
                ResourceId = resource.DRILL_2.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                ResourceCapabilityId = resourceSkill.DRILLING.Id,
                SetupTime = 5
            };

            DRILL_3_M4 = new M_ResourceSetup
            {
                Name = resource.DRILL_3.Name + " M4",
                ResourceId = resource.DRILL_3.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M4.Id,
                ResourceCapabilityId = resourceSkill.DRILLING.Id,
                SetupTime = 5
            };

            DRILL_3_M6 = new M_ResourceSetup
            {
                Name = resource.DRILL_3.Name + " M6",
                ResourceId = resource.DRILL_3.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                ResourceCapabilityId = resourceSkill.DRILLING.Id,
                SetupTime = 5
            };

            ASSEMBLY_1_SCREWDRIVER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_1.Name + " screwdriver universal",
                ResourceId = resource.ASSEMBLY_1.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_1_HOLDING = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_1.Name + " holding",
                ResourceId = resource.ASSEMBLY_1.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HOLDING.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_1_HAMMER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_1.Name + " hammer",
                ResourceId = resource.ASSEMBLY_1.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HAMMER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_2_SCREWDRIVER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_2.Name + " screwdriver universal",
                ResourceId = resource.ASSEMBLY_2.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_2_HOLDING = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_2.Name + " holding",
                ResourceId = resource.ASSEMBLY_2.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HOLDING.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_2_HAMMER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_2.Name + " hammer",
                ResourceId = resource.ASSEMBLY_2.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HAMMER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_3_SCREWDRIVER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_3.Name + " screwdriver universal",
                ResourceId = resource.ASSEMBLY_3.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_3_HOLDING = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_3.Name + " holding",
                ResourceId = resource.ASSEMBLY_3.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HOLDING.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_3_HAMMER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_3.Name + " hammer",
                ResourceId = resource.ASSEMBLY_3.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HAMMER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_4_SCREWDRIVER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_4.Name + " screwdriver universal",
                ResourceId = resource.ASSEMBLY_4.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_4_HOLDING = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_4.Name + " holding",
                ResourceId = resource.ASSEMBLY_4.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HOLDING.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_4_HAMMER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_4.Name + " hammer",
                ResourceId = resource.ASSEMBLY_4.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HAMMER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_5_SCREWDRIVER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_5.Name + " screwdriver universal",
                ResourceId = resource.ASSEMBLY_5.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_5_HOLDING = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_5.Name + " holding",
                ResourceId = resource.ASSEMBLY_5.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HOLDING.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_5_HAMMER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_5.Name + " hammer",
                ResourceId = resource.ASSEMBLY_5.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HAMMER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_6_SCREWDRIVER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_6.Name + " screwdriver universal",
                ResourceId = resource.ASSEMBLY_6.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_6_HOLDING = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_6.Name + " holding",
                ResourceId = resource.ASSEMBLY_6.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HOLDING.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };

            ASSEMBLY_6_HAMMER = new M_ResourceSetup
            {
                Name = resource.ASSEMBLY_6.Name + " hammer",
                ResourceId = resource.ASSEMBLY_6.Id,
                ResourceToolId = resourceTool.ASSEMBLY_HAMMER.Id,
                ResourceCapabilityId = resourceSkill.ASSEMBLING.Id,
                SetupTime = 10
            };
        }
        public M_ResourceSetup[] InitMedium(MasterDBContext context)
        {
            var resourceSetups = new M_ResourceSetup[]{
                SAW_1_BLADE_BIG,
                SAW_1_BLADE_SMALL,
                SAW_2_BLADE_BIG,
                SAW_2_BLADE_SMALL,
                DRILL_1_M4,
                DRILL_1_M6,
                ASSEMBLY_1_SCREWDRIVER,
                ASSEMBLY_1_HOLDING,
                ASSEMBLY_1_HAMMER,
                ASSEMBLY_2_SCREWDRIVER,
                ASSEMBLY_2_HOLDING,
                ASSEMBLY_2_HAMMER

            };
            context.ResourceSetups.AddRange(entities: resourceSetups);
            context.SaveChanges();
            return resourceSetups;
        }
        public M_ResourceSetup[] InitLarge(MasterDBContext context)
        {
            var resourceSetups = new M_ResourceSetup[]{
                SAW_1_BLADE_BIG,
                SAW_1_BLADE_SMALL,
                SAW_2_BLADE_BIG,
                SAW_2_BLADE_SMALL,
                SAW_3_BLADE_BIG,
                SAW_3_BLADE_SMALL,
                SAW_4_BLADE_BIG,
                SAW_4_BLADE_SMALL,
                SAW_5_BLADE_BIG,
                SAW_5_BLADE_SMALL,
                SAW_6_BLADE_BIG,
                SAW_6_BLADE_SMALL,
                DRILL_1_M4,
                DRILL_1_M6,
                DRILL_2_M4,
                DRILL_2_M6,
                DRILL_3_M4,
                DRILL_3_M6,
                ASSEMBLY_1_SCREWDRIVER,
                ASSEMBLY_1_HOLDING,
                ASSEMBLY_1_HAMMER,
                ASSEMBLY_2_SCREWDRIVER,
                ASSEMBLY_2_HOLDING,
                ASSEMBLY_2_HAMMER,
                ASSEMBLY_3_SCREWDRIVER,
                ASSEMBLY_3_HOLDING,
                ASSEMBLY_3_HAMMER,
                ASSEMBLY_4_SCREWDRIVER,
                ASSEMBLY_4_HOLDING,
                ASSEMBLY_4_HAMMER,
                ASSEMBLY_5_SCREWDRIVER,
                ASSEMBLY_5_HOLDING,
                ASSEMBLY_5_HAMMER,
                ASSEMBLY_6_SCREWDRIVER,
                ASSEMBLY_6_HOLDING,
                ASSEMBLY_6_HAMMER
            };
            context.ResourceSetups.AddRange(entities: resourceSetups);
            context.SaveChanges();
            return resourceSetups;
        }

    }
}