using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal static class MasterTableBom
    {
        // Final Products i think not needed anymore.                                                                        
        // new M_ArticleBom { ArticleChildId = MasterTableArticle.DUMP_TRUCK.Id, Name = "Dump-Truck" },
        // new M_ArticleBom { ArticleChildId = MasterTableArticle.RACE_TRUCK.Id, Name = "Race-Truck" },
        // Bom For DumpTruck
        internal static M_ArticleBom SKELETON_TO_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.SKELETON.Id, Name = "Skeleton", Quantity = 1,
            ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id, OperationId = MasterTableOperation.DUMP_TRUCK_WEDDING.Id
        };

        internal static M_ArticleBom CHASSIS_TYPE_DUMP_TO_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.CHASSIS_TYPE_DUMP.Id, Name = "Chassis Type: Dump", Quantity = 1,
            ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id, OperationId = MasterTableOperation.DUMP_TRUCK_WEDDING.Id
        };

        internal static M_ArticleBom GLUE_TO_DUMP_TRUCK_FOR_WEDDING = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.GLUE.Id, Name = "Glue", Quantity = 5,
            ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id, OperationId = MasterTableOperation.DUMP_TRUCK_WEDDING.Id
        };

        internal static M_ArticleBom POLE_TO_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.POLE.Id, Name = "Pole", Quantity = 1,
            ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id, OperationId = MasterTableOperation.GLUE_TRUCK_BED.Id
        };

        internal static M_ArticleBom TRUCK_BED_TO_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TRUCK_BED.Id, Name = "Truck-Bed", Quantity = 1,
            ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id, OperationId = MasterTableOperation.GLUE_TRUCK_BED.Id
        };

        internal static M_ArticleBom GLUE_TO_TO_DUMP_TRUCK_FOR_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.GLUE.Id, Name = "Glue", Quantity = 5,
            ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id, OperationId = MasterTableOperation.GLUE_TRUCK_BED.Id
        };

        internal static M_ArticleBom PEGS_TO_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.PEGS.Id, Name = "Pegs", Quantity = 2,
            ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id, OperationId = MasterTableOperation.GLUE_TRUCK_BED.Id
        };

        internal static M_ArticleBom BUTTON_TO_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.BUTTON.Id, Name = "Knopf", Quantity = 2,
            ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id, OperationId = MasterTableOperation.GLUE_TRUCK_BED.Id
        };
        // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id },
        // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = MasterTableArticle.DUMP_TRUCK.Id },
        
        // Bom For Race Truck
        internal static M_ArticleBom SKELETON_TO_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.SKELETON.Id, Name = "Skeleton", Quantity = 1,
            ArticleParentId = MasterTableArticle.RACE_TRUCK.Id, OperationId = MasterTableOperation.RACE_TRUCK_WEDDING.Id
        };

        internal static M_ArticleBom CHASSIS_TO_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.CHASSIS_TYPE_RACE.Id, Name = "Chassis Type: Race", Quantity = 1,
            ArticleParentId = MasterTableArticle.RACE_TRUCK.Id, OperationId = MasterTableOperation.RACE_TRUCK_WEDDING.Id
        };

        internal static M_ArticleBom POLE_TO_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.POLE.Id, Name = "Pole", Quantity = 1,
            ArticleParentId = MasterTableArticle.RACE_TRUCK.Id, OperationId = MasterTableOperation.GLUE_RACE_WING.Id
        };

        internal static M_ArticleBom RACE_WING_TO_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.RACE_WING.Id, Name = "Race Wing", Quantity = 1,
            ArticleParentId = MasterTableArticle.RACE_TRUCK.Id, OperationId = MasterTableOperation.GLUE_RACE_WING.Id
        };

        internal static M_ArticleBom GLUE_TO_RACE_TRUCK_FOR_WEDDING = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.GLUE.Id, Name = "Glue", Quantity = 5,
            ArticleParentId = MasterTableArticle.RACE_TRUCK.Id, OperationId = MasterTableOperation.RACE_TRUCK_WEDDING.Id
        };

        internal static M_ArticleBom GLUE_TO_RACE_TRUCK_FOR_WING = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.GLUE.Id, Name = "Glue", Quantity = 5,
            ArticleParentId = MasterTableArticle.RACE_TRUCK.Id, OperationId = MasterTableOperation.GLUE_RACE_WING.Id
        };

        internal static M_ArticleBom PEGS_TO_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.PEGS.Id, Name = "Pegs", Quantity = 2,
            ArticleParentId = MasterTableArticle.RACE_TRUCK.Id, OperationId = MasterTableOperation.GLUE_RACE_WING.Id
        };

        internal static M_ArticleBom BUTTON_TO_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.BUTTON.Id, Name = "Knopf", Quantity = 2,
            ArticleParentId = MasterTableArticle.RACE_TRUCK.Id, OperationId = MasterTableOperation.GLUE_RACE_WING.Id
        };
        // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "User Manual").Id, Name = "User Manual", Quantity=1, ArticleParentId = MasterTableArticle.RACE_TRUCK.Id },
        // new M_ArticleBom { ArticleChildId = articles.Single(predicate: a => a.Name == "Packing").Id, Name = "Packing", Quantity=1, ArticleParentId = MasterTableArticle.RACE_TRUCK.Id },
        
        // Bom for Skeleton
        internal static M_ArticleBom BASE_PLATE_TO_SKELETON = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.BASE_PLATE.Id, Name = "Base plate", Quantity = 1,
            ArticleParentId = MasterTableArticle.SKELETON.Id, OperationId = MasterTableOperation.MOUNT_AXIS.Id
        };

        internal static M_ArticleBom POLE_TO_SKELETON = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.POLE.Id, Name = "Pole", Quantity = 2,
            ArticleParentId = MasterTableArticle.SKELETON.Id, OperationId = MasterTableOperation.MOUNT_AXIS.Id
        };

        internal static M_ArticleBom WASHER_TO_SKELETON = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.WASHER.Id, Name = "Washer", Quantity = 4,
            ArticleParentId = MasterTableArticle.SKELETON.Id, OperationId = MasterTableOperation.SCREW_WHEELS.Id
        };

        internal static M_ArticleBom WHEEL_TO_SKELETON = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.WHEEL.Id, Name = "Wheel", Quantity = 4,
            ArticleParentId = MasterTableArticle.SKELETON.Id, OperationId = MasterTableOperation.SCREW_WHEELS.Id
        };

        internal static M_ArticleBom SEMITRAILER_TO_SKELETON = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.SEMITRAILER.Id, Name = "Semitrailer", Quantity = 1,
            ArticleParentId = MasterTableArticle.SKELETON.Id, OperationId = MasterTableOperation.GLUE_SEMITRAILER.Id
        };

        internal static M_ArticleBom GLUE_TO_SKELETON = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.GLUE.Id, Name = "Glue", Quantity = 5,
            ArticleParentId = MasterTableArticle.SKELETON.Id, OperationId = MasterTableOperation.GLUE_SEMITRAILER.Id
        };
        
        // Bom For Chassis Dump
        internal static M_ArticleBom CABIN_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.CABIN.Id, Name = "Cabin", Quantity = 1,
            ArticleParentId = MasterTableArticle.SKELETON.Id,
            OperationId = MasterTableOperation.DUMP_TRUCK_ASSEMBLE_LAMPS.Id
        };

        internal static M_ArticleBom PEGS_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.PEGS.Id, Name = "Pegs", Quantity = 4,
            ArticleParentId = MasterTableArticle.SKELETON.Id,
            OperationId = MasterTableOperation.DUMP_TRUCK_ASSEMBLE_LAMPS.Id
        };

        internal static M_ArticleBom BUTTON_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.BUTTON.Id, Name = "Knopf", Quantity = 2,
            ArticleParentId = MasterTableArticle.SKELETON.Id,
            OperationId = MasterTableOperation.DUMP_TRUCK_ASSEMBLE_LAMPS.Id
        };

        internal static M_ArticleBom ENGINE_BLOCK_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.ENGINE_BLOCK.Id, Name = "Engine-Block", Quantity = 1,
            ArticleParentId = MasterTableArticle.SKELETON.Id,
            OperationId = MasterTableOperation.DUMP_TRUCK_MOUNT_ENGINE.Id
        };

        internal static M_ArticleBom GLUE_TO_CHASSIS_DUMP_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.GLUE.Id, Name = "Glue", Quantity = 7,
            ArticleParentId = MasterTableArticle.SKELETON.Id,
            OperationId = MasterTableOperation.DUMP_TRUCK_MOUNT_ENGINE.Id
        };
        
        // Bom For Chassis Race
        internal static M_ArticleBom CABIN_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.CABIN.Id, Name = "Cabin", Quantity = 1,
            ArticleParentId = MasterTableArticle.CHASSIS_TYPE_RACE.Id,
            OperationId = MasterTableOperation.RACE_TRUCK_ASSEMBLE_LAMPS.Id
        };

        internal static M_ArticleBom PEGS_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.PEGS.Id, Name = "Pegs", Quantity = 4,
            ArticleParentId = MasterTableArticle.CHASSIS_TYPE_RACE.Id,
            OperationId = MasterTableOperation.RACE_TRUCK_ASSEMBLE_LAMPS.Id
        };

        internal static M_ArticleBom BUTTON_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.BUTTON.Id, Name = "Knopf", Quantity = 2,
            ArticleParentId = MasterTableArticle.CHASSIS_TYPE_RACE.Id,
            OperationId = MasterTableOperation.RACE_TRUCK_ASSEMBLE_LAMPS.Id
        };

        internal static M_ArticleBom ENGINE_BLOCK_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.ENGINE_BLOCK.Id, Name = "Engine-Block", Quantity = 1,
            ArticleParentId = MasterTableArticle.CHASSIS_TYPE_RACE.Id,
            OperationId = MasterTableOperation.RACE_TRUCK_MOUNT_ENGINE_EXTENSION.Id
        };

        internal static M_ArticleBom ENGINE_RACE_EXTENSION_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.ENGINE_RACE_EXTENSION.Id, Name = "Engine Race Extension", Quantity = 1,
            ArticleParentId = MasterTableArticle.CHASSIS_TYPE_RACE.Id,
            OperationId = MasterTableOperation.RACE_TRUCK_MOUNT_ENGINE_EXTENSION.Id
        };

        internal static M_ArticleBom GLUE_TO_CHASSIS_RACE_TRUCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.GLUE.Id, Name = "Glue", Quantity = 7,
            ArticleParentId = MasterTableArticle.CHASSIS_TYPE_RACE.Id,
            OperationId = MasterTableOperation.RACE_TRUCK_MOUNT_ENGINE.Id
        };
        
        // Bom for Truck-Bed
        internal static M_ArticleBom SIDEWALL_LONG_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.SIDEWALL_LONG.Id, Name = "Side wall long", Quantity = 2,
            ArticleParentId = MasterTableArticle.TRUCK_BED.Id, OperationId = MasterTableOperation.GLUE_SIDEWALLS.Id
        };

        internal static M_ArticleBom SIDEWALL_SHORT_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.SIDEWALL_SHORT.Id, Name = "Side wall short", Quantity = 2,
            ArticleParentId = MasterTableArticle.TRUCK_BED.Id, OperationId = MasterTableOperation.GLUE_SIDEWALLS.Id
        };

        internal static M_ArticleBom BASE_PLATE_TRUCK_BED_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.BASEPLATE_TRUCK_BED.Id, Name = "Base plate Truck-Bed", Quantity = 1,
            ArticleParentId = MasterTableArticle.TRUCK_BED.Id, OperationId = MasterTableOperation.GLUE_SIDEWALLS.Id
        };

        internal static M_ArticleBom DUMP_JOINT_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.DUMP_JOINT.Id, Name = "Dump Joint", Quantity = 1,
            ArticleParentId = MasterTableArticle.TRUCK_BED.Id, OperationId = MasterTableOperation.MOUNT_HATCHBACK.Id
        };

        internal static M_ArticleBom PEGS_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.PEGS.Id, Name = "Pegs", Quantity = 10,
            ArticleParentId = MasterTableArticle.TRUCK_BED.Id, OperationId = MasterTableOperation.MOUNT_HATCHBACK.Id
        };

        internal static M_ArticleBom BUTTON_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.BUTTON.Id, Name = "Knopf", Quantity = 2,
            ArticleParentId = MasterTableArticle.TRUCK_BED.Id, OperationId = MasterTableOperation.MOUNT_HATCHBACK.Id
        };

        internal static M_ArticleBom GLUE_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.GLUE.Id, Name = "Glue", Quantity = 7,
            ArticleParentId = MasterTableArticle.TRUCK_BED.Id, OperationId = MasterTableOperation.MOUNT_HATCHBACK.Id
        };

        internal static M_ArticleBom POLE_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.POLE.Id, Name = "Pole", Quantity = 1,
            ArticleParentId = MasterTableArticle.TRUCK_BED.Id, OperationId = MasterTableOperation.MOUNT_HATCHBACK.Id
        };
        
        // Bom for some Assemblies
        internal static M_ArticleBom TIMBER_PLATE_TO_SIDEWALL_LONG = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
            ArticleParentId = MasterTableArticle.SIDEWALL_LONG.Id,
            OperationId = MasterTableOperation.SIDEWALL_LONG_CUT.Id
        };

        internal static M_ArticleBom TIMBER_PLATE_TO_SIDEWALL_SHORT = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
            ArticleParentId = MasterTableArticle.SIDEWALL_SHORT.Id,
            OperationId = MasterTableOperation.SIDEWALL_SHORT_CUT.Id
        };

        internal static M_ArticleBom TIMBER_PLATE_TO_TRUCK_BED = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
            ArticleParentId = MasterTableArticle.BASEPLATE_TRUCK_BED.Id,
            OperationId = MasterTableOperation.BASEPLATE_TRUCK_BED_CUT.Id
        };

        internal static M_ArticleBom TIMBER_PLATE_TO_BASE_PLATE = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
            ArticleParentId = MasterTableArticle.BASE_PLATE.Id, OperationId = MasterTableOperation.BASE_PLATE_CUT.Id
        };

        internal static M_ArticleBom TIMBER_PLATE_TO_RACE_WING = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TIMBER_PLATE.Id, Name = "Timber Plate 1,5m x 3,0m", Quantity = 1,
            ArticleParentId = MasterTableArticle.RACE_WING.Id, OperationId = MasterTableOperation.RACE_WING_CUT.Id
        };

        internal static M_ArticleBom TIMBER_BLOCK_TO_CABIN = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TIMBER_BLOCK.Id, Name = "Timber Block 0,20m x 0,20m", Quantity = 1,
            ArticleParentId = MasterTableArticle.CABIN.Id, OperationId = MasterTableOperation.CABIN_CUT.Id
        };

        internal static M_ArticleBom TIMBER_BLOCK_TO_ENGINE_BLOCK = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TIMBER_BLOCK.Id, Name = "Timber Block 0,20m x 0,20m", Quantity = 1,
            ArticleParentId = MasterTableArticle.ENGINE_BLOCK.Id, OperationId = MasterTableOperation.ENGINE_BLOCK_CUT.Id
        };

        internal static M_ArticleBom TIMBER_BLOCK_TO_RACE_EXTENSION = new M_ArticleBom
        {
            ArticleChildId = MasterTableArticle.TIMBER_BLOCK.Id, Name = "Timber Block 0,20m x 0,20m", Quantity = 1,
            ArticleParentId = MasterTableArticle.ENGINE_RACE_EXTENSION.Id,
            OperationId = MasterTableOperation.RACE_EXTENSION_CUT.Id
        };

        internal static M_ArticleBom[] Init(MasterDBContext context)
        {
            
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