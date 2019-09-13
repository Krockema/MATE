using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using System;

namespace Master40.DB.Data.Initializer.Tables
{
    public static class MasterTableArticle
    {
        public static M_Article DUMP_TRUCK = new M_Article
        {
            Name = "Dump-Truck", ArticleTypeId = MasterTableArticleType.PRODUCT.Id,
            CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 20, UnitId = MasterTableUnit.PIECES.Id,
            Price = 45.00, ToPurchase = false, ToBuild = true, PictureUrl = "/images/Product/05_Truck_final.jpg"
        };

        public static M_Article RACE_TRUCK = new M_Article
        {
            Name = "Race-Truck",
            ArticleTypeId = MasterTableArticleType.PRODUCT.Id,
            CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 20,
            UnitId = MasterTableUnit.PIECES.Id, Price = 45.00, ToPurchase = false,
            ToBuild = true, PictureUrl = "/images/Product/06_Race-Truck_final.jpg"
        };

        public static M_Article SKELETON = new M_Article
        {
            Name = "Skeleton",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 15.00, ToPurchase = false,
            ToBuild = true, PictureUrl = "/images/Product/01_Bodenplatte.jpg"
        };

        public static M_Article TRUCK_BED = new M_Article
        {
            Name = "Truck-Bed",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 15.00, ToPurchase = false,
            ToBuild = true, PictureUrl = "/images/Product/03_Ladefläche.jpg"
        };

        public static M_Article CHASSIS_TYPE_DUMP = new M_Article
        {
            Name = "Chassis Type: Dump",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 15.00, ToPurchase = false,
            ToBuild = true, PictureUrl = "/images/Product/02_Gehäuse.jpg"
        };

        public static M_Article CHASSIS_TYPE_RACE = new M_Article
        {
            Name = "Chassis Type: Race",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 20.00, ToPurchase = false,
            ToBuild = true, PictureUrl = "/images/Product/08_Race-Truck_Chassie.jpg"
        };

        public static M_Article RACE_WING = new M_Article
        {
            Name = "Race Wing",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2016-09-01"), DeliveryPeriod = 5,
            UnitId = MasterTableUnit.PIECES.Id, Price = 5.00, ToPurchase = false,
            ToBuild = true, PictureUrl = "/images/Product/07_Race-Wing.jpg"
        };

        public static M_Article CABIN = new M_Article
        {
            Name = "Cabin",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 1.75, ToPurchase = false,
            ToBuild = true
        };

        public static M_Article ENGINE_BLOCK = new M_Article
        {
            Name = "Engine-Block",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 3.00, ToPurchase = false,
            ToBuild = true
        };
        
        // Truck Bed
        public static M_Article SIDEWALL_LONG = new M_Article
        {
            Name = "Side wall long",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 0.35, ToPurchase = false,
            ToBuild = true
        };

        public static M_Article SIDEWALL_SHORT = new M_Article
        {
            Name = "Side wall short",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 0.25, ToPurchase = false,
            ToBuild = true
        };

        public static M_Article BASEPLATE_TRUCK_BED = new M_Article
        {
            Name = "Base plate Truck-Bed",
            ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 0.40, ToPurchase = false,
            ToBuild = true
        };

        public static M_Article DUMP_JOINT = new M_Article
        {
            Name = "Dump Joint" /*Kippgelenk*/,
            ArticleTypeId = MasterTableArticleType.MATERIAL.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10,
            UnitId = MasterTableUnit.PIECES.Id, Price = 0.90, ToPurchase = true,
            ToBuild = false
        };
        
        // Engine Extension and Race Wing
        public static M_Article ENGINE_RACE_EXTENSION = new M_Article
        {
            Name = "Engine Race Extension", ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = MasterTableUnit.PIECES.Id,
            Price = 0.50, ToPurchase = false, ToBuild = true
        };
        // Skeleton
        public static M_Article WHEEL = new M_Article
        {
            Name = "Wheel", ArticleTypeId = MasterTableArticleType.MATERIAL.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = MasterTableUnit.PIECES.Id,
            Price = 1.00, ToPurchase = true, ToBuild = false
        };

        public static M_Article BASE_PLATE = new M_Article
        {
            Name = "Base plate", ArticleTypeId = MasterTableArticleType.ASSEMBLY.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = MasterTableUnit.PIECES.Id,
            Price = 0.80, ToPurchase = false, ToBuild = true
        };

        public static M_Article SEMITRAILER = new M_Article
        {
            Name = "Semitrailer" /*Aufleger*/, ArticleTypeId = MasterTableArticleType.MATERIAL.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = MasterTableUnit.PIECES.Id,
            Price = 0.10, ToPurchase = true, ToBuild = false
        };

        public static M_Article WASHER = new M_Article
        {
            Name = "Washer", ArticleTypeId = MasterTableArticleType.MATERIAL.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = MasterTableUnit.KILO.Id,
            Price = 0.02, ToPurchase = true, ToBuild = false
        };
        
        // base Materials
        public static M_Article TIMBER_PLATE = new M_Article
        {
            Name = "Timber Plate 1,5m x 3,0m", ArticleTypeId = MasterTableArticleType.MATERIAL.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 5, UnitId = MasterTableUnit.PIECES.Id,
            Price = 0.20, ToPurchase = true, ToBuild = false
        };

        public static M_Article TIMBER_BLOCK = new M_Article
        {
            Name = "Timber Block 0,20m x 0,20m", ArticleTypeId = MasterTableArticleType.MATERIAL.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 5, UnitId = MasterTableUnit.PIECES.Id,
            Price = 0.70, ToPurchase = true, ToBuild = false
        };

        public static M_Article GLUE = new M_Article
        {
            Name = "Glue", ArticleTypeId = MasterTableArticleType.CONSUMABLE.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = MasterTableUnit.LITER.Id,
            Price = 0.01, ToPurchase = true, ToBuild = false
        };

        public static M_Article PEGS = new M_Article
        {
            Name = "Pegs", ArticleTypeId = MasterTableArticleType.CONSUMABLE.Id,
            CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod = 3, UnitId = MasterTableUnit.KILO.Id,
            Price = 0.01, ToPurchase = true, ToBuild = false
        };

        public static M_Article POLE = new M_Article
        {
            Name = "Pole", ArticleTypeId = MasterTableArticleType.MATERIAL.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = MasterTableUnit.PIECES.Id,
            Price = 0.25, ToPurchase = true, ToBuild = false
        };

        public static M_Article BUTTON = new M_Article
        {
            Name = "Button", ArticleTypeId = MasterTableArticleType.MATERIAL.Id,
            CreationDate = DateTime.Parse(s: "2002-09-01"), DeliveryPeriod = 10, UnitId = MasterTableUnit.KILO.Id,
            Price = 0.05, ToPurchase = true, ToBuild = false
        };

        public static M_Article PACKING = new M_Article
        {
            Name = "Packing", ArticleTypeId = MasterTableArticleType.CONSUMABLE.Id,
            CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod = 4, UnitId = MasterTableUnit.KILO.Id,
            Price = 2.15, ToPurchase = true, ToBuild = false
        };

        public static M_Article USER_MANUAL = new M_Article
        {
            Name = "User Manual", ArticleTypeId = MasterTableArticleType.CONSUMABLE.Id,
            CreationDate = DateTime.Parse(s: "2005-09-01"), DeliveryPeriod = 4, UnitId = MasterTableUnit.KILO.Id,
            Price = 0.50, ToPurchase = true, ToBuild = false
        };


        public static M_Article[] Init(MasterDBContext context)
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
