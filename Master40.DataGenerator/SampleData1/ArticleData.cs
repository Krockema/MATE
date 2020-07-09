using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.SampleData1
{
    class ArticleData
    {
        public M_Article SCHRANK;
        public M_Article REGAL;

        public M_Article RAHMEN;
        public M_Article RAHMEN_MIT_RÜCKWAND;
        public M_Article SEITENWAND;
        public M_Article ABLAGE;
        public M_Article RÜCKWAND;
        public M_Article BODENPLATTE;
        public M_Article DECKPLATTE;
        public M_Article SCHRANKTÜR;

        public M_Article METALLDÜBEL;
        public M_Article HOLZBRETT_KURZ;
        public M_Article HOLZBRETT_LANG;
        public M_Article TÜRGRIFF;
        public M_Article SCHARNIERE;
        public M_Article SCHRAUBE;
        public M_Article NAGEL;
        public M_Article SPANPLATTE;
        public M_Article FUß;
        public M_Article KLEIDERSTANGE;
        public M_Article KLEIDERSTANGENHALTERUNG;

        internal ArticleData(MasterTableArticleType articleType, MasterTableUnit unit)
        {
            SCHRANK = new M_Article
            {
                Name = "Schrank",
                ArticleTypeId = articleType.PRODUCT.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 20,
                UnitId = unit.PIECES.Id,
                Price = 200,
                ToPurchase = false,
                ToBuild = true
            };

            REGAL = new M_Article
            {
                Name = "Regal",
                ArticleTypeId = articleType.PRODUCT.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 20,
                UnitId = unit.PIECES.Id,
                Price = 150,
                ToPurchase = false,
                ToBuild = true
            };

            RAHMEN = new M_Article
            {
                Name = "Rahmen",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id,
                Price = 80,
                ToPurchase = false,
                ToBuild = true
            };

            RAHMEN_MIT_RÜCKWAND = new M_Article
            {
                Name = "Rahmen mit Rückwand",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id,
                Price = 100,
                ToPurchase = false,
                ToBuild = true
            };

            SEITENWAND = new M_Article
            {
                Name = "Seitenwand",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id,
                Price = 30,
                ToPurchase = false,
                ToBuild = true
            };

            ABLAGE = new M_Article
            {
                Name = "Ablage",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id,
                Price = 10,
                ToPurchase = false,
                ToBuild = true
            };

            RÜCKWAND = new M_Article
            {
                Name = "Rückwand",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id,
                Price = 25,
                ToPurchase = false,
                ToBuild = true
            };

            BODENPLATTE = new M_Article
            {
                Name = "Bodenplatte",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id,
                Price = 15,
                ToPurchase = false,
                ToBuild = true
            };

            DECKPLATTE = new M_Article
            {
                Name = "Deckplatte",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id,
                Price = 10,
                ToPurchase = false,
                ToBuild = true
            };

            SCHRANKTÜR = new M_Article
            {
                Name = "Schranktür",
                ArticleTypeId = articleType.ASSEMBLY.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 10,
                UnitId = unit.PIECES.Id,
                Price = 40,
                ToPurchase = false,
                ToBuild = true
            };

            METALLDÜBEL = new M_Article
            {
                Name = "Metalldübel",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 0.5,
                ToPurchase = true,
                ToBuild = false
            };

            HOLZBRETT_KURZ = new M_Article
            {
                Name = "kurzes Holzbrett",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 8,
                ToPurchase = true,
                ToBuild = false
            };

            HOLZBRETT_LANG = new M_Article
            {
                Name = "langes Holzbrett",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 25,
                ToPurchase = true,
                ToBuild = false
            };

            TÜRGRIFF = new M_Article
            {
                Name = "Türgriff",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 2,
                ToPurchase = true,
                ToBuild = false
            };

            SCHARNIERE = new M_Article
            {
                Name = "Scharniere",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 5,
                ToPurchase = true,
                ToBuild = false
            };

            SCHRAUBE = new M_Article
            {
                Name = "Schraube",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 0.2,
                ToPurchase = true,
                ToBuild = false
            };

            NAGEL = new M_Article
            {
                Name = "Nagel",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 0.1,
                ToPurchase = true,
                ToBuild = false
            };

            SPANPLATTE = new M_Article
            {
                Name = "Spanplatte",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 20,
                ToPurchase = true,
                ToBuild = false
            };

            FUß = new M_Article
            {
                Name = "Fuß",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 2,
                ToPurchase = true,
                ToBuild = false
            };

            KLEIDERSTANGE = new M_Article
            {
                Name = "Kleiderstange",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 5,
                ToPurchase = true,
                ToBuild = false
            };

            KLEIDERSTANGENHALTERUNG = new M_Article
            {
                Name = "Kleiderstangenhalterung",
                ArticleTypeId = articleType.MATERIAL.Id,
                CreationDate = DateTime.Parse(s: "2020-07-07"),
                DeliveryPeriod = 5,
                UnitId = unit.PIECES.Id,
                Price = 0.5,
                ToPurchase = true,
                ToBuild = false
            };

        }

        public M_Article[] Init(MasterDBContext context)
        {
            var articles = new M_Article[]
            {
                //Endprodukte
                SCHRANK,
                REGAL,

                //Zwischenprodukte
                RAHMEN,
                RAHMEN_MIT_RÜCKWAND,
                SEITENWAND,
                ABLAGE,
                RÜCKWAND,
                BODENPLATTE,
                DECKPLATTE,
                SCHRANKTÜR,

                //Ausgangsmaterialien
                METALLDÜBEL,
                HOLZBRETT_KURZ,
                HOLZBRETT_LANG,
                TÜRGRIFF,
                SCHARNIERE,
                SCHRAUBE,
                NAGEL,
                SPANPLATTE,
                FUß,
                KLEIDERSTANGE,
                KLEIDERSTANGENHALTERUNG
            };

            context.Articles.AddRange(entities: articles);
            context.SaveChanges();
            return articles;
        }

    }
}
