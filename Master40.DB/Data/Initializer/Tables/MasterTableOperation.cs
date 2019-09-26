using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableOperation
    {
         // assemble Truck
         internal M_Operation DUMP_TRUCK_WEDDING;

         internal M_Operation GLUE_TRUCK_BED;
         
         // assemble Truck
         internal M_Operation RACE_TRUCK_WEDDING;

         internal M_Operation GLUE_RACE_WING;
         
         // assemble chassie Dump-Truck
         internal M_Operation DUMP_TRUCK_ASSEMBLE_LAMPS;

         internal M_Operation DUMP_TRUCK_MOUNT_ENGINE;
         
         // assemble chassie Race Truck
         internal M_Operation RACE_TRUCK_ASSEMBLE_LAMPS;

         internal M_Operation RACE_TRUCK_MOUNT_ENGINE_EXTENSION;

         internal M_Operation RACE_TRUCK_MOUNT_ENGINE;
         
         // assemble Skeleton
         internal M_Operation MOUNT_AXIS;

         internal M_Operation SCREW_WHEELS;

         internal M_Operation GLUE_SEMITRAILER;
         
         // assemble Truck Bed
         internal M_Operation GLUE_SIDEWALLS;

         internal M_Operation MOUNT_HATCHBACK;
         
         // assemble Race Wing
         internal M_Operation RACE_WING_CUT;

         internal M_Operation RACE_WING_DRILL;
         // Engine Race Extension
         internal M_Operation RACE_EXTENSION_CUT;

         internal M_Operation RACE_EXTENSION_DRILL;
         
         // side Walls for Truck-bed
         internal M_Operation SIDEWALL_LONG_CUT;

         internal M_Operation SIDEWALL_LONG_DRILL;

         internal M_Operation SIDEWALL_SHORT_CUT;

         internal M_Operation SIDEWALL_SHORT_DRILL;

         internal M_Operation BASEPLATE_TRUCK_BED_CUT;

         internal M_Operation BASEPLATE_TRUCK_BED_DRILL;
         // Eengin Block
         internal M_Operation ENGINE_BLOCK_CUT;

         internal M_Operation ENGINE_BLOCK_DRILL;
         // cabin 
         internal M_Operation CABIN_CUT;

         internal M_Operation CABIN_DRILL;
         // Base Plate
         internal M_Operation BASE_PLATE_CUT;

         internal M_Operation BASE_PLATE_DRILL;

         internal MasterTableOperation(MasterTableArticle articles
                                     , MasterTableResourceSkill resourceSkill
                                     , MasterTableResourceTool resourceTool)
         {
             DUMP_TRUCK_WEDDING = new M_Operation
             {
                 ArticleId = articles.DUMP_TRUCK.Id,
                 Name = "Dump-Truck: Wedding",
                 Duration = 15,
                 ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                 ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                 HierarchyNumber = 10
             };

            GLUE_TRUCK_BED = new M_Operation
            {
                ArticleId = articles.DUMP_TRUCK.Id,
                Name = "Glue Truck-Bed",
                Duration = 10,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 20
            };

            // assemble Truck
            RACE_TRUCK_WEDDING = new M_Operation
            {
                ArticleId = articles.RACE_TRUCK.Id,
                Name = "Race-Truck: Wedding",
                Duration = 15,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 10
            };

            GLUE_RACE_WING = new M_Operation
            {
                ArticleId = articles.RACE_TRUCK.Id,
                Name = "Glue Race Wing",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 20
            };

            // assemble chassie Dump-Truck
            DUMP_TRUCK_ASSEMBLE_LAMPS = new M_Operation
            {
                ArticleId = articles.CHASSIS_TYPE_DUMP.Id,
                Name = "Dump-Truck: Assemble Lamps",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 10
            };

            DUMP_TRUCK_MOUNT_ENGINE = new M_Operation
            {
                ArticleId = articles.CHASSIS_TYPE_DUMP.Id,
                Name = "Dump-Truck: Mount Engine to Cabin",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 20
            };

            // assemble chassie Race Truck
            RACE_TRUCK_ASSEMBLE_LAMPS = new M_Operation
            {
                ArticleId = articles.CHASSIS_TYPE_RACE.Id,
                Name = "Race-Truck: Assemble Lamps",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 10
            };

            RACE_TRUCK_MOUNT_ENGINE_EXTENSION = new M_Operation
            {
                ArticleId = articles.CHASSIS_TYPE_RACE.Id,
                Name = "Mount Engine Extension",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 20
            };

            RACE_TRUCK_MOUNT_ENGINE = new M_Operation
            {
                ArticleId = articles.CHASSIS_TYPE_RACE.Id,
                Name = "Race-Truck: Mount Engine to Cabin",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 30
            };

            // assemble Skeleton
            MOUNT_AXIS = new M_Operation
            {
                ArticleId = articles.SKELETON.Id,
                Name = "mount poles with wheels to Skeleton",
                Duration = 10,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 10
            };

            SCREW_WHEELS = new M_Operation
            {
                ArticleId = articles.SKELETON.Id,
                Name = "Screw wheels onto poles",
                Duration = 10,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 20
            };

            GLUE_SEMITRAILER = new M_Operation
            {
                ArticleId = articles.SKELETON.Id,
                Name = "Glue Semitrailer",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 30
            };

            // assemble Truck Bed
            GLUE_SIDEWALLS = new M_Operation
            {
                ArticleId = articles.TRUCK_BED.Id,
                Name = "Glue side walls and base plate together",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 10
            };

            MOUNT_HATCHBACK = new M_Operation
            {
                ArticleId = articles.TRUCK_BED.Id,
                Name = "Mount hatchback",
                Duration = 5,
                ResourceSkillId = resourceSkill.ASSEMBLING.Id,
                ResourceToolId = resourceTool.ASSEMBLY_SCREWDRIVER.Id,
                HierarchyNumber = 20
            };

            // assemble Race Wing
            RACE_WING_CUT = new M_Operation
            {
                ArticleId = articles.RACE_WING.Id,
                Name = "Race Wing: Cut shape",
                Duration = 10,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                HierarchyNumber = 10
            };

            RACE_WING_DRILL = new M_Operation
            {
                ArticleId = articles.RACE_WING.Id,
                Name = "Race Wing: Drill Mount Holes",
                Duration = 5,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                HierarchyNumber = 20
            };
            // Engine Race Extension
            RACE_EXTENSION_CUT = new M_Operation
            {
                ArticleId = articles.ENGINE_RACE_EXTENSION.Id,
                Name = "Engine Race Extension: Cut shape",
                Duration = 10,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                HierarchyNumber = 10
            };

            RACE_EXTENSION_DRILL = new M_Operation
            {
                ArticleId = articles.ENGINE_RACE_EXTENSION.Id,
                Name = "Engine Race Extension: Drill Mount Holes",
                Duration = 5,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                HierarchyNumber = 20
            };

            // side Walls for Truck-bed
            SIDEWALL_LONG_CUT = new M_Operation
            {
                ArticleId = articles.SIDEWALL_LONG.Id,
                Name = "Side wall long: Cut long side",
                Duration = 10,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                HierarchyNumber = 10
            };

            SIDEWALL_LONG_DRILL = new M_Operation
            {
                ArticleId = articles.SIDEWALL_LONG.Id,
                Name = "Side wall long: Drill mount holes",
                Duration = 5,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                HierarchyNumber = 20
            };

            SIDEWALL_SHORT_CUT = new M_Operation
            {
                ArticleId = articles.SIDEWALL_SHORT.Id,
                Name = "Side wall short: Cut short side",
                Duration = 5,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                HierarchyNumber = 10
            };

            SIDEWALL_SHORT_DRILL = new M_Operation
            {
                ArticleId = articles.SIDEWALL_SHORT.Id,
                Name = "Side wall short: Drill mount holes",
                Duration = 5,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                HierarchyNumber = 20
            };

            BASEPLATE_TRUCK_BED_CUT = new M_Operation
            {
                ArticleId = articles.BASEPLATE_TRUCK_BED.Id,
                Name = "Base plate Truck-Bed: Cut Base plate Truck-Bed",
                Duration = 10,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                HierarchyNumber = 10
            };

            BASEPLATE_TRUCK_BED_DRILL = new M_Operation
            {
                ArticleId = articles.BASEPLATE_TRUCK_BED.Id,
                Name = "Base plate Truck-Bed: Drill mount holes",
                Duration = 5,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                HierarchyNumber = 20
            };
            // Eengin Block
            ENGINE_BLOCK_CUT = new M_Operation
            {
                ArticleId = articles.ENGINE_BLOCK.Id,
                Name = "Engine-Block: Cut Engine-Block",
                Duration = 10,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                HierarchyNumber = 10
            };

            ENGINE_BLOCK_DRILL = new M_Operation
            {
                ArticleId = articles.ENGINE_BLOCK.Id,
                Name = "Engine-Block: Drill mount holes",
                Duration = 5,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M6.Id,
                HierarchyNumber = 20
            };
            // cabin 
            CABIN_CUT = new M_Operation
            {
                ArticleId = articles.CABIN.Id,
                Name = "Cabin: Cut Cabin",
                Duration = 10,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                ResourceToolId = resourceTool.SAW_BLADE_SMALL.Id,
                HierarchyNumber = 10
            };

            CABIN_DRILL = new M_Operation
            {
                ArticleId = articles.CABIN.Id,
                Name = "Cabin: Drill mount holes",
                Duration = 5,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M4.Id,
                HierarchyNumber = 20
            };
            // Base Plate
            BASE_PLATE_CUT = new M_Operation
            {
                ArticleId = articles.BASE_PLATE.Id,
                Name = "Base plate: Cut Base plate",
                Duration = 10,
                ResourceSkillId = resourceSkill.CUTTING.Id,
                ResourceToolId = resourceTool.SAW_BLADE_BIG.Id,
                HierarchyNumber = 10
            };

            BASE_PLATE_DRILL = new M_Operation
            {
                ArticleId = articles.BASE_PLATE.Id,
                Name = "Base plate: drill holes for axis mount",
                Duration = 5,
                ResourceSkillId = resourceSkill.DRILLING.Id,
                ResourceToolId = resourceTool.DRILL_HEAD_M4.Id,
                HierarchyNumber = 20
            };
        }



        internal M_Operation[] Init(MasterDBContext context)
        {
            var operations = new M_Operation[] {                   
                DUMP_TRUCK_WEDDING,
                GLUE_TRUCK_BED,
                RACE_TRUCK_WEDDING,
                GLUE_RACE_WING,

                DUMP_TRUCK_ASSEMBLE_LAMPS,
                DUMP_TRUCK_MOUNT_ENGINE,

                RACE_TRUCK_ASSEMBLE_LAMPS,
                RACE_TRUCK_MOUNT_ENGINE_EXTENSION,
                RACE_TRUCK_MOUNT_ENGINE,

                MOUNT_AXIS,
                SCREW_WHEELS,
                GLUE_SEMITRAILER,

                GLUE_SIDEWALLS,
                MOUNT_HATCHBACK,

                RACE_WING_CUT,
                RACE_WING_DRILL,

                RACE_EXTENSION_CUT,
                RACE_EXTENSION_DRILL,

                SIDEWALL_LONG_CUT,
                SIDEWALL_LONG_DRILL,

                SIDEWALL_SHORT_CUT,
                SIDEWALL_SHORT_DRILL,

                BASEPLATE_TRUCK_BED_CUT,
                BASEPLATE_TRUCK_BED_DRILL,

                ENGINE_BLOCK_CUT,
                ENGINE_BLOCK_DRILL,

                CABIN_CUT,
                CABIN_DRILL,

                BASE_PLATE_CUT,
                BASE_PLATE_DRILL
            };
            context.Operations.AddRange(entities: operations);
            context.SaveChanges();
            return operations;
        }
        
    }
}