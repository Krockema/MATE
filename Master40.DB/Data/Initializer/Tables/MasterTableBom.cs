using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableBom
    {
        // Final Products i think not needed anymore.                                                                        
        // new M_ArticleBom { ArticleChildId = articles.DUMP_TRUCK.Id, Name = "Dump-Truck" },
        // new M_ArticleBom { ArticleChildId = articles.RACE_TRUCK.Id, Name = "Race-Truck" },
        // Bom For DumpTruck
        internal M_ArticleBom SKELETON_TO_DUMP_TRUCK;

        internal M_ArticleBom CHASSIS_TYPE_DUMP_TO_DUMP_TRUCK;

        internal M_ArticleBom GLUE_TO_DUMP_TRUCK_FOR_WEDDING;

        internal M_ArticleBom POLE_TO_DUMP_TRUCK;

        internal M_ArticleBom TRUCK_BED_TO_DUMP_TRUCK;

        internal M_ArticleBom GLUE_TO_TO_DUMP_TRUCK_FOR_TRUCK_BED;

        internal M_ArticleBom PEGS_TO_DUMP_TRUCK;

        internal M_ArticleBom BUTTON_TO_DUMP_TRUCK;
        // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = articles.DUMP_TRUCK.Id },
        // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.DUMP_TRUCK.Id },

        // Bom For Race Truck
        internal M_ArticleBom SKELETON_TO_RACE_TRUCK;

        internal M_ArticleBom CHASSIS_TO_RACE_TRUCK;

        internal M_ArticleBom POLE_TO_RACE_TRUCK;

        internal M_ArticleBom RACE_WING_TO_RACE_TRUCK;

        internal M_ArticleBom GLUE_TO_RACE_TRUCK_FOR_WEDDING;

        internal M_ArticleBom GLUE_TO_RACE_TRUCK_FOR_WING;

        internal M_ArticleBom PEGS_TO_RACE_TRUCK;

        internal M_ArticleBom BUTTON_TO_RACE_TRUCK;
        // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = articles.RACE_TRUCK.Id },
        // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.RACE_TRUCK.Id },

        // Bom for Skeleton
        internal M_ArticleBom BASE_PLATE_TO_SKELETON;

        internal M_ArticleBom POLE_TO_SKELETON;

        internal M_ArticleBom WASHER_TO_SKELETON;

        internal M_ArticleBom WHEEL_TO_SKELETON;

        internal M_ArticleBom SEMITRAILER_TO_SKELETON;

        internal M_ArticleBom GLUE_TO_SKELETON;

        // Bom For Chassis Dump
        internal M_ArticleBom CABIN_TO_CHASSIS_DUMP_TRUCK;

        internal M_ArticleBom PEGS_TO_CHASSIS_DUMP_TRUCK;

        internal M_ArticleBom BUTTON_TO_CHASSIS_DUMP_TRUCK;

        internal M_ArticleBom ENGINE_BLOCK_TO_CHASSIS_DUMP_TRUCK;

        internal M_ArticleBom GLUE_TO_CHASSIS_DUMP_TRUCK;

        // Bom For Chassis Race
        internal M_ArticleBom CABIN_TO_CHASSIS_RACE_TRUCK;

        internal M_ArticleBom PEGS_TO_CHASSIS_RACE_TRUCK;

        internal M_ArticleBom BUTTON_TO_CHASSIS_RACE_TRUCK;

        internal M_ArticleBom ENGINE_BLOCK_TO_CHASSIS_RACE_TRUCK;

        internal M_ArticleBom ENGINE_RACE_EXTENSION_TO_CHASSIS_RACE_TRUCK;

        internal M_ArticleBom GLUE_TO_CHASSIS_RACE_TRUCK;

        // Bom for Truck-Bed
        internal M_ArticleBom SIDEWALL_LONG_TO_TRUCK_BED;

        internal M_ArticleBom SIDEWALL_SHORT_TO_TRUCK_BED;

        internal M_ArticleBom BASE_PLATE_TRUCK_BED_TO_TRUCK_BED;

        internal M_ArticleBom DUMP_JOINT_TO_TRUCK_BED;

        internal M_ArticleBom PEGS_TO_TRUCK_BED;

        internal M_ArticleBom BUTTON_TO_TRUCK_BED;

        internal M_ArticleBom GLUE_TO_TRUCK_BED;

        internal M_ArticleBom POLE_TO_TRUCK_BED;

        // Bom for some Assemblies
        internal M_ArticleBom TIMBER_PLATE_TO_SIDEWALL_LONG;

        internal M_ArticleBom TIMBER_PLATE_TO_SIDEWALL_SHORT;

        internal M_ArticleBom TIMBER_PLATE_TO_TRUCK_BED;

        internal M_ArticleBom TIMBER_PLATE_TO_BASE_PLATE;

        internal M_ArticleBom TIMBER_PLATE_TO_RACE_WING;

        internal M_ArticleBom TIMBER_BLOCK_TO_CABIN;

        internal M_ArticleBom TIMBER_BLOCK_TO_ENGINE_BLOCK;

        internal M_ArticleBom TIMBER_BLOCK_TO_RACE_EXTENSION;

        internal M_ArticleBom[] Init(MasterDBContext context, MasterTableArticle articles, MasterTableOperation operations)
        {
            SKELETON_TO_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.SKELETON.Id, Name = "Skeleton", Quantity = 1,
                ArticleParentId = articles.DUMP_TRUCK.Id, OperationId = operations.DUMP_TRUCK_WEDDING.Id
            };

            CHASSIS_TYPE_DUMP_TO_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.CHASSIS_TYPE_DUMP.Id, Name = "Chassis Type: Dump", Quantity = 1,
                ArticleParentId = articles.DUMP_TRUCK.Id, OperationId = operations.DUMP_TRUCK_WEDDING.Id
            };

            GLUE_TO_DUMP_TRUCK_FOR_WEDDING = new M_ArticleBom
            {
                ArticleChildId = articles.GLUE.Id, Name = "Glue", Quantity = 5,
                ArticleParentId = articles.DUMP_TRUCK.Id, OperationId = operations.DUMP_TRUCK_WEDDING.Id
            };

            POLE_TO_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.POLE.Id, Name = "Pole", Quantity = 1,
                ArticleParentId = articles.DUMP_TRUCK.Id, OperationId = operations.GLUE_TRUCK_BED.Id
            };

            TRUCK_BED_TO_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.TRUCK_BED.Id, Name = "Truck-Bed", Quantity = 1,
                ArticleParentId = articles.DUMP_TRUCK.Id, OperationId = operations.GLUE_TRUCK_BED.Id
            };

            GLUE_TO_TO_DUMP_TRUCK_FOR_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.GLUE.Id, Name = "Glue", Quantity = 5,
                ArticleParentId = articles.DUMP_TRUCK.Id, OperationId = operations.GLUE_TRUCK_BED.Id
            };

            PEGS_TO_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.PEGS.Id, Name = "Pegs", Quantity = 2,
                ArticleParentId = articles.DUMP_TRUCK.Id, OperationId = operations.GLUE_TRUCK_BED.Id
            };

            BUTTON_TO_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.BUTTON.Id, Name = "Knopf", Quantity = 2,
                ArticleParentId = articles.DUMP_TRUCK.Id, OperationId = operations.GLUE_TRUCK_BED.Id
            };
            // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = articles.DUMP_TRUCK.Id },
            // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.DUMP_TRUCK.Id },
            
            // Bom For Race Truck
            SKELETON_TO_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.SKELETON.Id, Name = "Skeleton", Quantity = 1,
                ArticleParentId = articles.RACE_TRUCK.Id, OperationId = operations.RACE_TRUCK_WEDDING.Id
            };

            CHASSIS_TO_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.CHASSIS_TYPE_RACE.Id, Name = "Chassis Type: Race", Quantity = 1,
                ArticleParentId = articles.RACE_TRUCK.Id, OperationId = operations.RACE_TRUCK_WEDDING.Id
            };

            POLE_TO_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.POLE.Id, Name = "Pole", Quantity = 1,
                ArticleParentId = articles.RACE_TRUCK.Id, OperationId = operations.GLUE_RACE_WING.Id
            };

            RACE_WING_TO_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.RACE_WING.Id, Name = "Race Wing", Quantity = 1,
                ArticleParentId = articles.RACE_TRUCK.Id, OperationId = operations.GLUE_RACE_WING.Id
            };

            GLUE_TO_RACE_TRUCK_FOR_WEDDING = new M_ArticleBom
            {
                ArticleChildId = articles.GLUE.Id, Name = "Glue", Quantity = 5,
                ArticleParentId = articles.RACE_TRUCK.Id, OperationId = operations.RACE_TRUCK_WEDDING.Id
            };

            GLUE_TO_RACE_TRUCK_FOR_WING = new M_ArticleBom
            {
                ArticleChildId = articles.GLUE.Id, Name = "Glue", Quantity = 5,
                ArticleParentId = articles.RACE_TRUCK.Id, OperationId = operations.GLUE_RACE_WING.Id
            };

            PEGS_TO_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.PEGS.Id, Name = "Pegs", Quantity = 2,
                ArticleParentId = articles.RACE_TRUCK.Id, OperationId = operations.GLUE_RACE_WING.Id
            };

            BUTTON_TO_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.BUTTON.Id, Name = "Knopf", Quantity = 2,
                ArticleParentId = articles.RACE_TRUCK.Id, OperationId = operations.GLUE_RACE_WING.Id
            };
            // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = articles.RACE_TRUCK.Id },
            // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = articles.RACE_TRUCK.Id },
            
            // Bom for Skeleton
            BASE_PLATE_TO_SKELETON = new M_ArticleBom
            {
                ArticleChildId = articles.BASE_PLATE.Id, Name = "Base plate", Quantity = 1,
                ArticleParentId = articles.SKELETON.Id, OperationId = operations.MOUNT_AXIS.Id
            };

            POLE_TO_SKELETON = new M_ArticleBom
            {
                ArticleChildId = articles.POLE.Id, Name = "Pole", Quantity = 2,
                ArticleParentId = articles.SKELETON.Id, OperationId = operations.MOUNT_AXIS.Id
            };

            WASHER_TO_SKELETON = new M_ArticleBom
            {
                ArticleChildId = articles.WASHER.Id, Name = "Washer", Quantity = 4,
                ArticleParentId = articles.SKELETON.Id, OperationId = operations.SCREW_WHEELS.Id
            };

            WHEEL_TO_SKELETON = new M_ArticleBom
            {
                ArticleChildId = articles.WHEEL.Id, Name = "Wheel", Quantity = 4,
                ArticleParentId = articles.SKELETON.Id, OperationId = operations.SCREW_WHEELS.Id
            };

            SEMITRAILER_TO_SKELETON = new M_ArticleBom
            {
                ArticleChildId = articles.SEMITRAILER.Id, Name = "Semitrailer", Quantity = 1,
                ArticleParentId = articles.SKELETON.Id, OperationId = operations.GLUE_SEMITRAILER.Id
            };

            GLUE_TO_SKELETON = new M_ArticleBom
            {
                ArticleChildId = articles.GLUE.Id, Name = "Glue", Quantity = 5,
                ArticleParentId = articles.SKELETON.Id, OperationId = operations.GLUE_SEMITRAILER.Id
            };
            
            // Bom For Chassis Dump
            CABIN_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.CABIN.Id, Name = "Cabin", Quantity = 1,
                ArticleParentId = articles.CHASSIS_TYPE_DUMP.Id,
                OperationId = operations.DUMP_TRUCK_ASSEMBLE_LAMPS.Id
            };

            PEGS_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.PEGS.Id, Name = "Pegs", Quantity = 4,
                ArticleParentId = articles.CHASSIS_TYPE_DUMP.Id,
                OperationId = operations.DUMP_TRUCK_ASSEMBLE_LAMPS.Id
            };

            BUTTON_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.BUTTON.Id, Name = "Knopf", Quantity = 2,
                ArticleParentId = articles.CHASSIS_TYPE_DUMP.Id,
                OperationId = operations.DUMP_TRUCK_ASSEMBLE_LAMPS.Id
            };

            ENGINE_BLOCK_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.ENGINE_BLOCK.Id, Name = "Engine-Block", Quantity = 1,
                ArticleParentId = articles.CHASSIS_TYPE_DUMP.Id,
                OperationId = operations.DUMP_TRUCK_MOUNT_ENGINE.Id
            };

            GLUE_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.GLUE.Id, Name = "Glue", Quantity = 7,
                ArticleParentId = articles.CHASSIS_TYPE_DUMP.Id,
                OperationId = operations.DUMP_TRUCK_MOUNT_ENGINE.Id
            };
            
            // Bom For Chassis Race
            CABIN_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.CABIN.Id, Name = "Cabin", Quantity = 1,
                ArticleParentId = articles.CHASSIS_TYPE_RACE.Id,
                OperationId = operations.RACE_TRUCK_ASSEMBLE_LAMPS.Id
            };

            PEGS_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.PEGS.Id, Name = "Pegs", Quantity = 4,
                ArticleParentId = articles.CHASSIS_TYPE_RACE.Id,
                OperationId = operations.RACE_TRUCK_ASSEMBLE_LAMPS.Id
            };

            BUTTON_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.BUTTON.Id, Name = "Knopf", Quantity = 2,
                ArticleParentId = articles.CHASSIS_TYPE_RACE.Id,
                OperationId = operations.RACE_TRUCK_ASSEMBLE_LAMPS.Id
            };

            ENGINE_BLOCK_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.ENGINE_BLOCK.Id, Name = "Engine-Block", Quantity = 1,
                ArticleParentId = articles.CHASSIS_TYPE_RACE.Id,
                OperationId = operations.RACE_TRUCK_MOUNT_ENGINE_EXTENSION.Id
            };

            ENGINE_RACE_EXTENSION_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.ENGINE_RACE_EXTENSION.Id, Name = "Engine Race Extension", Quantity = 1,
                ArticleParentId = articles.CHASSIS_TYPE_RACE.Id,
                OperationId = operations.RACE_TRUCK_MOUNT_ENGINE_EXTENSION.Id
            };

            GLUE_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
            {
                ArticleChildId = articles.GLUE.Id, Name = "Glue", Quantity = 7,
                ArticleParentId = articles.CHASSIS_TYPE_RACE.Id,
                OperationId = operations.RACE_TRUCK_MOUNT_ENGINE.Id
            };
            
            // Bom for Truck-Bed
            SIDEWALL_LONG_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.SIDEWALL_LONG.Id, Name = "Side wall long", Quantity = 2,
                ArticleParentId = articles.TRUCK_BED.Id, OperationId = operations.GLUE_SIDEWALLS.Id
            };

            SIDEWALL_SHORT_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.SIDEWALL_SHORT.Id, Name = "Side wall short", Quantity = 2,
                ArticleParentId = articles.TRUCK_BED.Id, OperationId = operations.GLUE_SIDEWALLS.Id
            };

            BASE_PLATE_TRUCK_BED_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.BASEPLATE_TRUCK_BED.Id, Name = "Base plate Truck-Bed", Quantity = 1,
                ArticleParentId = articles.TRUCK_BED.Id, OperationId = operations.GLUE_SIDEWALLS.Id
            };

            DUMP_JOINT_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.DUMP_JOINT.Id, Name = "Dump Joint", Quantity = 1,
                ArticleParentId = articles.TRUCK_BED.Id, OperationId = operations.MOUNT_HATCHBACK.Id
            };

            PEGS_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.PEGS.Id, Name = "Pegs", Quantity = 10,
                ArticleParentId = articles.TRUCK_BED.Id, OperationId = operations.MOUNT_HATCHBACK.Id
            };

            BUTTON_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.BUTTON.Id, Name = "Knopf", Quantity = 2,
                ArticleParentId = articles.TRUCK_BED.Id, OperationId = operations.MOUNT_HATCHBACK.Id
            };

            GLUE_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.GLUE.Id, Name = "Glue", Quantity = 7,
                ArticleParentId = articles.TRUCK_BED.Id, OperationId = operations.MOUNT_HATCHBACK.Id
            };

            POLE_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.POLE.Id, Name = "Pole", Quantity = 1,
                ArticleParentId = articles.TRUCK_BED.Id, OperationId = operations.MOUNT_HATCHBACK.Id
            };
            
            // Bom for some Assemblies
            TIMBER_PLATE_TO_SIDEWALL_LONG = new M_ArticleBom
            {
                ArticleChildId = articles.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
                ArticleParentId = articles.SIDEWALL_LONG.Id,
                OperationId = operations.SIDEWALL_LONG_CUT.Id
            };

            TIMBER_PLATE_TO_SIDEWALL_SHORT = new M_ArticleBom
            {
                ArticleChildId = articles.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
                ArticleParentId = articles.SIDEWALL_SHORT.Id,
                OperationId = operations.SIDEWALL_SHORT_CUT.Id
            };

            TIMBER_PLATE_TO_TRUCK_BED = new M_ArticleBom
            {
                ArticleChildId = articles.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
                ArticleParentId = articles.BASEPLATE_TRUCK_BED.Id,
                OperationId = operations.BASEPLATE_TRUCK_BED_CUT.Id
            };

            TIMBER_PLATE_TO_BASE_PLATE = new M_ArticleBom
            {
                ArticleChildId = articles.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
                ArticleParentId = articles.BASE_PLATE.Id, OperationId = operations.BASE_PLATE_CUT.Id
            };

            TIMBER_PLATE_TO_RACE_WING = new M_ArticleBom
            {
                ArticleChildId = articles.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
                ArticleParentId = articles.RACE_WING.Id, OperationId = operations.RACE_WING_CUT.Id
            };

            TIMBER_BLOCK_TO_CABIN = new M_ArticleBom
            {
                ArticleChildId = articles.TIMBER_BLOCK.Id, Name = "Timber Block 0,20m x 0,20m", Quantity = 1,
                ArticleParentId = articles.CABIN.Id, OperationId = operations.CABIN_CUT.Id
            };

            TIMBER_BLOCK_TO_ENGINE_BLOCK = new M_ArticleBom
            {
                ArticleChildId = articles.TIMBER_BLOCK.Id, Name = "Timber Block 0,20m x 0,20m", Quantity = 1,
                ArticleParentId = articles.ENGINE_BLOCK.Id, OperationId = operations.ENGINE_BLOCK_CUT.Id
            };

            TIMBER_BLOCK_TO_RACE_EXTENSION = new M_ArticleBom
            {
                ArticleChildId = articles.TIMBER_BLOCK.Id, Name = "Timber Block 0,20m x 0,20m", Quantity = 1,
                ArticleParentId = articles.ENGINE_RACE_EXTENSION.Id,
                OperationId = operations.RACE_EXTENSION_CUT.Id
            };
            // !!! - Important NOTE - !!!
            // For Boms without Link to an Operation all Materials have to be ready to completed the operation assigned to the Article.
            var articleBom = new M_ArticleBom[]
            {
                // DUMP TRUCK
                CHASSIS_TYPE_DUMP_TO_DUMP_TRUCK,
                SKELETON_TO_DUMP_TRUCK,
                GLUE_TO_DUMP_TRUCK_FOR_WEDDING,
                POLE_TO_DUMP_TRUCK,
                TRUCK_BED_TO_DUMP_TRUCK,
                GLUE_TO_TO_DUMP_TRUCK_FOR_TRUCK_BED,
                PEGS_TO_DUMP_TRUCK,
                BUTTON_TO_DUMP_TRUCK,
                //RACE TRUCK
                SKELETON_TO_RACE_TRUCK,
                CHASSIS_TO_RACE_TRUCK,
                POLE_TO_RACE_TRUCK,
                RACE_WING_TO_RACE_TRUCK,
                GLUE_TO_RACE_TRUCK_FOR_WEDDING,
                GLUE_TO_RACE_TRUCK_FOR_WING,
                PEGS_TO_RACE_TRUCK,
                BUTTON_TO_RACE_TRUCK,
                // SKELETON
                BASE_PLATE_TO_SKELETON,
                POLE_TO_SKELETON,
                WASHER_TO_SKELETON,
                WHEEL_TO_SKELETON,
                SEMITRAILER_TO_SKELETON,
                GLUE_TO_SKELETON,
                // CHASSIS DUMP TRUCK
                CABIN_TO_CHASSIS_DUMP_TRUCK,
                PEGS_TO_CHASSIS_DUMP_TRUCK,
                BUTTON_TO_CHASSIS_DUMP_TRUCK,
                ENGINE_BLOCK_TO_CHASSIS_DUMP_TRUCK,
                GLUE_TO_CHASSIS_DUMP_TRUCK,
                // CHASSIS RACE TRUCK
                CABIN_TO_CHASSIS_RACE_TRUCK,
                PEGS_TO_CHASSIS_RACE_TRUCK,
                BUTTON_TO_CHASSIS_RACE_TRUCK,
                ENGINE_BLOCK_TO_CHASSIS_RACE_TRUCK,
                ENGINE_RACE_EXTENSION_TO_CHASSIS_RACE_TRUCK,
                GLUE_TO_CHASSIS_RACE_TRUCK,
                // TRUCK BED
                SIDEWALL_LONG_TO_TRUCK_BED,
                SIDEWALL_SHORT_TO_TRUCK_BED,
                BASE_PLATE_TRUCK_BED_TO_TRUCK_BED,
                DUMP_JOINT_TO_TRUCK_BED,
                PEGS_TO_TRUCK_BED,
                BUTTON_TO_TRUCK_BED,
                GLUE_TO_TRUCK_BED,
                POLE_TO_TRUCK_BED,
                //ASSEMBLIES
                TIMBER_PLATE_TO_SIDEWALL_LONG,
                TIMBER_PLATE_TO_SIDEWALL_SHORT,
                TIMBER_PLATE_TO_TRUCK_BED,
                TIMBER_PLATE_TO_BASE_PLATE,
                TIMBER_PLATE_TO_RACE_WING,
                TIMBER_BLOCK_TO_CABIN,
                TIMBER_BLOCK_TO_ENGINE_BLOCK,
                TIMBER_BLOCK_TO_RACE_EXTENSION



            };
            context.ArticleBoms.AddRange(entities: articleBom);
            context.SaveChanges();
            return articleBom;
        }
        
    }
}