using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.SampleData1
{
    class BomData
    {

        internal M_ArticleBom SPANPLATTE_FÜR_RÜCKWAND;
        internal M_ArticleBom HOLZBRETT_FÜR_BODENPLATTE;
        internal M_ArticleBom SCHRAUBEN_FÜR_BODENPLATTE;
        internal M_ArticleBom FÜßE_FÜR_BODENPLATTE;
        internal M_ArticleBom HOLZBRETT_FÜR_ABLAGE;
        internal M_ArticleBom HOLZBRETT_FÜR_SEITENWAND;
        internal M_ArticleBom HOLZBRETT_FÜR_SCHRANKTÜR;
        internal M_ArticleBom TÜRGRIFF_FÜR_SCHRANKTÜR;
        internal M_ArticleBom SCHRAUBEN_FÜR_SCHRANKTÜR;
        internal M_ArticleBom SCHANIEREN_FÜR_SCHRANKTÜR;
        internal M_ArticleBom SCHRAUBEN_FÜR_RAHMEN;
        internal M_ArticleBom SEITENWAND_FÜR_RAHMEN;
        internal M_ArticleBom BODENPLATTE_FÜR_RAHMEN;
        internal M_ArticleBom DECKPLATTE_FÜR_RAHMEN;
        internal M_ArticleBom RAHMEN_FÜR_RAHMEN_MIT_RÜCKWAND;
        internal M_ArticleBom RÜCKWAND_FÜR_RAHMEN_MIT_RÜCKWAND;
        internal M_ArticleBom DÜBEL_FÜR_REGAL;
        internal M_ArticleBom RAHMEN_MIT_RÜCKWAND_FÜR_REGAL;
        internal M_ArticleBom ABLAGEN_FÜR_REGAL;
        internal M_ArticleBom RAHMEN_MIT_RÜCKWAND_FÜR_SCHRANK;
        internal M_ArticleBom KLEIDERSTANGENHALTERUNGEN_FÜR_SCHRANK;
        internal M_ArticleBom SCHRAUBEN_FÜR_KLEIDERSTANGENANBRINGUNG_AN_SCHRANK;
        internal M_ArticleBom KLEIDERSTANGE_FÜR_SCHRANK;
        internal M_ArticleBom NAÄGEL_FÜR_RAHMEN_MIT_RÜCKWAND;
        internal M_ArticleBom SCHRANKTÜREN_FÜR_SCHRANK;
        internal M_ArticleBom SCHRAUBEN_FÜR_SCHRANKTÜRBEFESTIGUNG;

        internal M_ArticleBom[] Init(MasterDBContext context, ArticleData articles, OperationData operations)
        {

            SPANPLATTE_FÜR_RÜCKWAND = new M_ArticleBom
            {
                ArticleChildId = articles.SPANPLATTE.Id,
                Name = "Spanplatte",
                Quantity = 1,
                ArticleParentId = articles.RÜCKWAND.Id,
                OperationId = operations.RÜCKWAND_ZUSAMMENBAUEN.Id
            };

            HOLZBRETT_FÜR_BODENPLATTE = new M_ArticleBom
            {
                ArticleChildId = articles.HOLZBRETT_KURZ.Id,
                Name = "kurzes Holzbrett",
                Quantity = 1,
                ArticleParentId = articles.BODENPLATTE.Id,
                OperationId = operations.BODENPLATTE_BOHREN.Id
            };

            SCHRAUBEN_FÜR_BODENPLATTE = new M_ArticleBom
            {
                ArticleChildId = articles.SCHRAUBE.Id,
                Name = "Schraube",
                Quantity = 4,
                ArticleParentId = articles.BODENPLATTE.Id,
                OperationId = operations.BODENPLATTE_ZUSAMMENBAUEN.Id
            };

            FÜßE_FÜR_BODENPLATTE = new M_ArticleBom
            {
                ArticleChildId = articles.FUß.Id,
                Name = "Fuß",
                Quantity = 4,
                ArticleParentId = articles.BODENPLATTE.Id,
                OperationId = operations.BODENPLATTE_ZUSAMMENBAUEN.Id
            };

            HOLZBRETT_FÜR_ABLAGE = new M_ArticleBom
            {
                ArticleChildId = articles.HOLZBRETT_KURZ.Id,
                Name = "kurzes Holzbrett",
                Quantity = 1,
                ArticleParentId = articles.ABLAGE.Id,
                OperationId = operations.ABLAGE_ZUSAMMENBAUEN.Id
            };

            HOLZBRETT_FÜR_SEITENWAND = new M_ArticleBom
            {
                ArticleChildId = articles.HOLZBRETT_LANG.Id,
                Name = "langes Holzbrett",
                Quantity = 1,
                ArticleParentId = articles.SEITENWAND.Id,
                OperationId = operations.SEITENWAND_BOHREN.Id
            };

            HOLZBRETT_FÜR_SCHRANKTÜR = new M_ArticleBom
            {
                ArticleChildId = articles.HOLZBRETT_LANG.Id,
                Name = "langes Holzbrett",
                Quantity = 1,
                ArticleParentId = articles.SCHRANKTÜR.Id,
                OperationId = operations.SCHRANKTÜR_BOHREN.Id
            };

            SCHRAUBEN_FÜR_SCHRANKTÜR = new M_ArticleBom
            {
                ArticleChildId = articles.SCHRAUBE.Id,
                Name = "Schraube",
                Quantity = 8,
                ArticleParentId = articles.SCHRANKTÜR.Id,
                OperationId = operations.SCHRANKTÜR_ZUSAMMENBAUEN.Id
            };

            TÜRGRIFF_FÜR_SCHRANKTÜR = new M_ArticleBom
            {
                ArticleChildId = articles.TÜRGRIFF.Id,
                Name = "Türgriff",
                Quantity = 1,
                ArticleParentId = articles.SCHRANKTÜR.Id,
                OperationId = operations.SCHRANKTÜR_ZUSAMMENBAUEN.Id
            };

            SCHANIEREN_FÜR_SCHRANKTÜR = new M_ArticleBom
            {
                ArticleChildId = articles.SCHARNIERE.Id,
                Name = "Schaniere",
                Quantity = 3,
                ArticleParentId = articles.SCHRANKTÜR.Id,
                OperationId = operations.SCHRANKTÜR_ZUSAMMENBAUEN.Id
            };

            SCHRAUBEN_FÜR_RAHMEN = new M_ArticleBom
            {
                ArticleChildId = articles.SCHRAUBE.Id,
                Name = "Schraube",
                Quantity = 8,
                ArticleParentId = articles.RAHMEN.Id,
                OperationId = operations.RAHMEN_ZUSAMMENBAUEN.Id
            };

            SEITENWAND_FÜR_RAHMEN = new M_ArticleBom
            {
                ArticleChildId = articles.SEITENWAND.Id,
                Name = "Seitenwand",
                Quantity = 2,
                ArticleParentId = articles.RAHMEN.Id,
                OperationId = operations.RAHMEN_ZUSAMMENBAUEN.Id
            };

            BODENPLATTE_FÜR_RAHMEN = new M_ArticleBom
            {
                ArticleChildId = articles.BODENPLATTE.Id,
                Name = "Bodenplatte",
                Quantity = 1,
                ArticleParentId = articles.RAHMEN.Id,
                OperationId = operations.RAHMEN_ZUSAMMENBAUEN.Id
            };

            DECKPLATTE_FÜR_RAHMEN = new M_ArticleBom
            {
                ArticleChildId = articles.DECKPLATTE.Id,
                Name = "Deckplatte",
                Quantity = 1,
                ArticleParentId = articles.RAHMEN.Id,
                OperationId = operations.RAHMEN_ZUSAMMENBAUEN.Id
            };

            RAHMEN_FÜR_RAHMEN_MIT_RÜCKWAND = new M_ArticleBom
            {
                ArticleChildId = articles.RAHMEN.Id,
                Name = "Rahmen",
                Quantity = 1,
                ArticleParentId = articles.RAHMEN_MIT_RÜCKWAND.Id,
                OperationId = operations.RÜCKWAND_MONTIEREN.Id
            };

            RÜCKWAND_FÜR_RAHMEN_MIT_RÜCKWAND = new M_ArticleBom
            {
                ArticleChildId = articles.RÜCKWAND.Id,
                Name = "Rückwand",
                Quantity = 1,
                ArticleParentId = articles.RAHMEN_MIT_RÜCKWAND.Id,
                OperationId = operations.RÜCKWAND_MONTIEREN.Id
            };

            NAÄGEL_FÜR_RAHMEN_MIT_RÜCKWAND = new M_ArticleBom
            {
                ArticleChildId = articles.NAGEL.Id,
                Name = "Nagel",
                Quantity = 80,
                ArticleParentId = articles.RAHMEN_MIT_RÜCKWAND.Id,
                OperationId = operations.RÜCKWAND_MONTIEREN.Id
            };

            DÜBEL_FÜR_REGAL = new M_ArticleBom
            {
                ArticleChildId = articles.METALLDÜBEL.Id,
                Name = "Metalldübel",
                Quantity = 16,
                ArticleParentId = articles.REGAL.Id,
                OperationId = operations.REGAL_DÜBEL_ANBRINGEN.Id
            };

            RAHMEN_MIT_RÜCKWAND_FÜR_REGAL = new M_ArticleBom
            {
                ArticleChildId = articles.RAHMEN_MIT_RÜCKWAND.Id,
                Name = "Rahmen mit Rückwand",
                Quantity = 1,
                ArticleParentId = articles.REGAL.Id,
                OperationId = operations.REGAL_DÜBEL_ANBRINGEN.Id
            };

            ABLAGEN_FÜR_REGAL = new M_ArticleBom
            {
                ArticleChildId = articles.ABLAGE.Id,
                Name = "Ablage",
                Quantity = 4,
                ArticleParentId = articles.REGAL.Id,
                OperationId = operations.REGAL_ABLAGEN_EINBAUEN.Id
            };

            RAHMEN_MIT_RÜCKWAND_FÜR_SCHRANK = new M_ArticleBom
            {
                ArticleChildId = articles.RAHMEN_MIT_RÜCKWAND.Id,
                Name = "Rahmen mit Rückwand",
                Quantity = 1,
                ArticleParentId = articles.SCHRANK.Id,
                OperationId = operations.SCHRANK_KLEIDERSTANGENHALTERUNGEN_ANBRINGEN.Id
            };

            KLEIDERSTANGENHALTERUNGEN_FÜR_SCHRANK = new M_ArticleBom
            {
                ArticleChildId = articles.KLEIDERSTANGENHALTERUNG.Id,
                Name = "Kleiderstangenhalterung",
                Quantity = 2,
                ArticleParentId = articles.SCHRANK.Id,
                OperationId = operations.SCHRANK_KLEIDERSTANGENHALTERUNGEN_ANBRINGEN.Id
            };

            SCHRAUBEN_FÜR_KLEIDERSTANGENANBRINGUNG_AN_SCHRANK = new M_ArticleBom
            {
                ArticleChildId = articles.SCHRAUBE.Id,
                Name = "Schraube",
                Quantity = 4,
                ArticleParentId = articles.SCHRANK.Id,
                OperationId = operations.SCHRANK_KLEIDERSTANGENHALTERUNGEN_ANBRINGEN.Id
            };

            KLEIDERSTANGE_FÜR_SCHRANK = new M_ArticleBom
            {
                ArticleChildId = articles.KLEIDERSTANGE.Id,
                Name = "Kleiderstange",
                Quantity = 1,
                ArticleParentId = articles.SCHRANK.Id,
                OperationId = operations.SCHRANK_KLEIDERSTANGE_EINBAUEN.Id
            };

            SCHRANKTÜREN_FÜR_SCHRANK = new M_ArticleBom
            {
                ArticleChildId = articles.SCHRANKTÜR.Id,
                Name = "Schranktür",
                Quantity = 2,
                ArticleParentId = articles.SCHRANK.Id,
                OperationId = operations.SCHRANKTÜR_BEFESTIGEN.Id
            };

            SCHRAUBEN_FÜR_SCHRANKTÜRBEFESTIGUNG = new M_ArticleBom
            {
                ArticleChildId = articles.SCHRAUBE.Id,
                Name = "Schraube",
                Quantity = 12,
                ArticleParentId = articles.SCHRANK.Id,
                OperationId = operations.SCHRANKTÜR_BEFESTIGEN.Id
            };

            var articleBom = new M_ArticleBom[]
            {
                HOLZBRETT_FÜR_BODENPLATTE,
                SCHRAUBEN_FÜR_BODENPLATTE,
                FÜßE_FÜR_BODENPLATTE,
                HOLZBRETT_FÜR_ABLAGE,
                HOLZBRETT_FÜR_SEITENWAND,
                SPANPLATTE_FÜR_RÜCKWAND,
                HOLZBRETT_FÜR_SCHRANKTÜR,
                TÜRGRIFF_FÜR_SCHRANKTÜR,
                SCHRAUBEN_FÜR_SCHRANKTÜR,
                SCHANIEREN_FÜR_SCHRANKTÜR,
                SCHRAUBEN_FÜR_RAHMEN,
                SEITENWAND_FÜR_RAHMEN,
                BODENPLATTE_FÜR_RAHMEN,
                DECKPLATTE_FÜR_RAHMEN,
                RAHMEN_FÜR_RAHMEN_MIT_RÜCKWAND,
                RÜCKWAND_FÜR_RAHMEN_MIT_RÜCKWAND,
                DÜBEL_FÜR_REGAL,
                RAHMEN_MIT_RÜCKWAND_FÜR_REGAL,
                ABLAGEN_FÜR_REGAL,
                RAHMEN_MIT_RÜCKWAND_FÜR_SCHRANK,
                KLEIDERSTANGENHALTERUNGEN_FÜR_SCHRANK,
                SCHRAUBEN_FÜR_KLEIDERSTANGENANBRINGUNG_AN_SCHRANK,
                KLEIDERSTANGE_FÜR_SCHRANK,
                NAÄGEL_FÜR_RAHMEN_MIT_RÜCKWAND,
                SCHRANKTÜREN_FÜR_SCHRANK,
                SCHRAUBEN_FÜR_SCHRANKTÜRBEFESTIGUNG
            };
            context.ArticleBoms.AddRange(entities: articleBom);
            context.SaveChanges();
            return articleBom;
        }

    }
}
