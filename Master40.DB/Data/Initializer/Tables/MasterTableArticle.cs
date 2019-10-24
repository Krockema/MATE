using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using System;
using Master40.DB.Interfaces;

namespace Master40.DB.Data.Initializer.Tables
{
    public class MasterTableArticle
    {
        public const string ARTICLE_PRODUCTS = "Product";

        public M_Article DUMP_TRUCK;

        public M_Article RACE_TRUCK;

        public M_Article SKELETON;

        public M_Article TRUCK_BED;

        public M_Article CHASSIS_TYPE_DUMP;

        public M_Article CHASSIS_TYPE_RACE;

        public M_Article RACE_WING;

        public M_Article CABIN;

        public M_Article ENGINE_BLOCK;

        // Truck Bed
        public M_Article SIDEWALL_LONG;

        public M_Article SIDEWALL_SHORT;

        public M_Article BASEPLATE_TRUCK_BED;

        public M_Article DUMP_JOINT;

        // Engine Extension and Race Wing
        public M_Article ENGINE_RACE_EXTENSION;
        // Skeleton
        public M_Article WHEEL;

        public M_Article BASE_PLATE;

        public M_Article SEMITRAILER;

        public M_Article WASHER;

        // base Materials
        public M_Article TIMBER_PLATE;

        public M_Article TIMBER_BLOCK;

        public M_Article GLUE;

        public M_Article PEGS;

        public M_Article POLE;

        public M_Article BUTTON;

        public M_Article PACKING;

        public M_Article USER_MANUAL;

        internal MasterTableArticle(MasterTableArticleType articleType, MasterTableUnit unit)
        {
            DUMP_TRUCK = new M_Article
            {
                Name = "Dump-Truck", ArticleTypeId = articleType.PRODUCT.Id,
                CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 20, UnitId = unit.PIECES.Id,
                Price = 45.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"
            };

            RACE_TRUCK = new M_Article
            {
                Name = "Race-Truck",
                ArticleTypeId = articleType.PRODUCT.Id,
                CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 20,
                UnitId = unit.PIECES.Id, Price = 45.00, ToPurchase = false,
                ToBuild = true, PictureUrl = "/images/Product/06_Race-Truck_final.jpg"
            };

            SKELETON = new M_Article
            {
                Name = "Skeleton",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 15.00, ToPurchase = false,
                ToBuild = true, PictureUrl = "/images/Product/01_Bodenplatte.jpg"
            };

            TRUCK_BED = new M_Article
            {
                Name = "Truck-Bed",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 15.00, ToPurchase = false,
                ToBuild = true, PictureUrl = "/images/Product/03_Ladefläche.jpg"
            };

            CHASSIS_TYPE_DUMP = new M_Article
            {
                Name = "Chassis Type: Dump",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 15.00, ToPurchase = false,
                ToBuild = true, PictureUrl = "/images/Product/02_Gehäuse.jpg"
            };

            CHASSIS_TYPE_RACE = new M_Article
            {
                Name = "Chassis Type: Race",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 20.00, ToPurchase = false,
                ToBuild = true, PictureUrl = "/images/Product/08_Race-Truck_Chassie.jpg"
            };

            RACE_WING = new M_Article
            {
                Name = "Race Wing",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id, Price = 5.00, ToPurchase = false,
                ToBuild = true, PictureUrl = "/images/Product/07_Race-Wing.jpg"
            };

            CABIN = new M_Article
            {
                Name = "Cabin",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 1.75, ToPurchase = false,
                ToBuild = true
            };

            ENGINE_BLOCK = new M_Article
            {
                Name = "Engine-Block",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 3.00, ToPurchase = false,
                ToBuild = true
            };
            
            // Truck Bed
            SIDEWALL_LONG = new M_Article
            {
                Name = "Side wall long",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 0.35, ToPurchase = false,
                ToBuild = true
            };

            SIDEWALL_SHORT = new M_Article
            {
                Name = "Side wall short",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 0.25, ToPurchase = false,
                ToBuild = true
            };

            BASEPLATE_TRUCK_BED = new M_Article
            {
                Name = "Base plate Truck-Bed",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 0.40, ToPurchase = false,
                ToBuild = true
            };

            DUMP_JOINT = new M_Article
            {
                Name = "Dump Joint" /*Kippgelenk*/,
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id, Price = 0.90, ToPurchase = true,
                ToBuild = false
            };
            
            // Engine Extension and Race Wing
            ENGINE_RACE_EXTENSION = new M_Article
            {
                Name = "Engine Race Extension", ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = unit.PIECES.Id,
                Price = 0.50, ToPurchase = false, ToBuild = true
            };
            // Skeleton
            WHEEL = new M_Article
            {
                Name = "Wheel", ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = unit.PIECES.Id,
                Price = 1.00, ToPurchase = true, ToBuild = false
            };

            BASE_PLATE = new M_Article
            {
                Name = "Base plate", ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = unit.PIECES.Id,
                Price = 0.80, ToPurchase = false, ToBuild = true
            };

            SEMITRAILER = new M_Article
            {
                Name = "Semitrailer" /*Aufleger*/, ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = unit.PIECES.Id,
                Price = 0.10, ToPurchase = true, ToBuild = false
            };

            WASHER = new M_Article
            {
                Name = "Washer", ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = unit.KILO.Id,
                Price = 0.02, ToPurchase = true, ToBuild = false
            };
            
            // base Materials
            TIMBER_PLATE = new M_Article
            {
                Name = "Timber Plate 1,5m x 3,0m", ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 5, UnitId = unit.PIECES.Id,
                Price = 0.20, ToPurchase = true, ToBuild = false
            };

            TIMBER_BLOCK = new M_Article
            {
                Name = "Timber Block 0,20m x 0,20m", ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 5, UnitId = unit.PIECES.Id,
                Price = 0.70, ToPurchase = true, ToBuild = false
            };

            GLUE = new M_Article
            {
                Name = "Glue", ArticleTypeId = articleType.CONSUMABLE.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = unit.LITER.Id,
                Price = 0.01, ToPurchase = true, ToBuild = false
            };

            PEGS = new M_Article
            {
                Name = "Pegs", ArticleTypeId = articleType.CONSUMABLE.Id,
                CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod = 3, UnitId = unit.KILO.Id,
                Price = 0.01, ToPurchase = true, ToBuild = false
            };

            POLE = new M_Article
            {
                Name = "Pole", ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = unit.PIECES.Id,
                Price = 0.25, ToPurchase = true, ToBuild = false
            };

            BUTTON = new M_Article
            {
                Name = "Button", ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = unit.KILO.Id,
                Price = 0.05, ToPurchase = true, ToBuild = false
            };

            PACKING = new M_Article
            {
                Name = "Packing", ArticleTypeId = articleType.CONSUMABLE.Id,
                CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod = 4, UnitId = unit.KILO.Id,
                Price = 2.15, ToPurchase = true, ToBuild = false
            };

            USER_MANUAL = new M_Article
            {
                Name = "User Manual", ArticleTypeId = articleType.CONSUMABLE.Id,
                CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod = 4, UnitId = unit.KILO.Id,
                Price = 0.50, ToPurchase = true, ToBuild = false
            };
        }

        public M_Article[] Init(MasterDBContext context)
        {
            var articles = new M_Article[]
            {
                // Final Product
                DUMP_TRUCK,
                RACE_TRUCK,
        
                // Cabin
                SKELETON,
                TRUCK_BED,
                CHASSIS_TYPE_DUMP,
                CHASSIS_TYPE_RACE,
                RACE_WING,
                CABIN,
                ENGINE_BLOCK,

                // Truck Bed
                SIDEWALL_LONG,
                SIDEWALL_SHORT,
                BASEPLATE_TRUCK_BED,
                DUMP_JOINT,

                ENGINE_RACE_EXTENSION,

                // Skeleton
                WHEEL,
                BASE_PLATE,
                SEMITRAILER,
                WASHER,

                // base Materials
                TIMBER_PLATE,
                TIMBER_BLOCK,
                GLUE, PEGS, POLE, BUTTON, PACKING, USER_MANUAL
            };

            context.Articles.AddRange(entities: articles);
            context.SaveChanges();
            return articles;
        }
    }
}
