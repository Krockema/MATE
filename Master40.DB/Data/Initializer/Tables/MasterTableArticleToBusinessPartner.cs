using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableArticleToBusinessPartner
    {
        internal static void Init(MasterDBContext context)
        {
            var artToBusinessPartner = new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.SKELETON.Id, PackSize = 10, Price = 20.00,
                    TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.TRUCK_BED.Id, PackSize = 10, Price = 20.00,
                    TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.CHASSIS_TYPE_DUMP.Id, PackSize = 10,
                    Price = 20.00, TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.CHASSIS_TYPE_RACE.Id, PackSize = 10,
                    Price = 25.00, TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.CABIN.Id, PackSize = 10, Price = 1.75,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.ENGINE_BLOCK.Id, PackSize = 10,
                    Price = 0.40, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.ENGINE_RACE_EXTENSION.Id, PackSize = 10,
                    Price = 1.00, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.SIDEWALL_LONG.Id, PackSize = 10,
                    Price = 0.55, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.SIDEWALL_SHORT.Id, PackSize = 10,
                    Price = 0.45, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.BASEPLATE_TRUCK_BED.Id, PackSize = 10,
                    Price = 0.40, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.DUMP_JOINT.Id /*Kippgelenk*/,
                    PackSize = 50, Price = 0.90, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.WHEEL.Id, PackSize = 150, Price = 0.35,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.BASE_PLATE.Id, PackSize = 10, Price = 0.80,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.SEMITRAILER.Id /*Aufleger*/ , PackSize = 25,
                    Price = 0.10, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.RACE_WING.Id, PackSize = 10, Price = 1.50,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.WASHER.Id, PackSize = 150, Price = 0.02,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.TIMBER_PLATE.Id,
                    PackSize = 100, Price = 0.20, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.TIMBER_BLOCK.Id,
                    PackSize = 100, Price = 0.20, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.GLUE.Id, PackSize = 1000, Price = 0.01,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.PEGS.Id, PackSize = 200, Price = 0.01,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.POLE.Id, PackSize = 200, Price = 0.25,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.BUTTON.Id, PackSize = 500, Price = 0.05,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.PACKING.Id, PackSize = 50, Price = 2.50,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = MasterTableBusinessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = MasterTableArticle.USER_MANUAL.Id, PackSize = 50,
                    Price = 0.20, TimeToDelivery = 1440
                },

            };
            context.ArticleToBusinessPartners.AddRange(entities: artToBusinessPartner);
            context.SaveChanges();
        }
    }
}