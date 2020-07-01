using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableArticleToBusinessPartner
    {
        internal void Init(MasterDBContext context, MasterTableBusinessPartner businessPartner, MasterTableArticle article)
        {
            var artToBusinessPartner = new M_ArticleToBusinessPartner[]
            {
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.SKELETON.Id, PackSize = 10, Price = 20.00,
                    TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.TRUCK_BED.Id, PackSize = 10, Price = 20.00,
                    TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.CHASSIS_TYPE_DUMP.Id, PackSize = 10,
                    Price = 20.00, TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.CHASSIS_TYPE_RACE.Id, PackSize = 10,
                    Price = 25.00, TimeToDelivery = 2880
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.CABIN.Id, PackSize = 10, Price = 1.75,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.ENGINE_BLOCK.Id, PackSize = 10,
                    Price = 0.40, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.ENGINE_RACE_EXTENSION.Id, PackSize = 10,
                    Price = 1.00, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.SIDEWALL_LONG.Id, PackSize = 10,
                    Price = 0.55, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.SIDEWALL_SHORT.Id, PackSize = 10,
                    Price = 0.45, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.BASEPLATE_TRUCK_BED.Id, PackSize = 10,
                    Price = 0.40, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.DUMP_JOINT.Id /*Kippgelenk*/,
                    PackSize = 50, Price = 0.90, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.WHEEL.Id, PackSize = 150, Price = 0.35,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.BASE_PLATE.Id, PackSize = 10, Price = 0.80,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.SEMITRAILER.Id /*Aufleger*/ , PackSize = 25,
                    Price = 0.10, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.RACE_WING.Id, PackSize = 10, Price = 1.50,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.WASHER.Id, PackSize = 150, Price = 0.02,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.TIMBER_PLATE.Id,
                    PackSize = 100, Price = 0.20, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.TIMBER_BLOCK.Id,
                    PackSize = 100, Price = 0.20, TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.GLUE.Id, PackSize = 1000, Price = 0.01,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.PEGS.Id, PackSize = 200, Price = 0.01,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.POLE.Id, PackSize = 200, Price = 0.25,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.BUTTON.Id, PackSize = 500, Price = 0.05,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.PACKING.Id, PackSize = 50, Price = 2.50,
                    TimeToDelivery = 1440
                },
                new M_ArticleToBusinessPartner
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.USER_MANUAL.Id, PackSize = 50,
                    Price = 0.20, TimeToDelivery = 1440
                },

            };
            context.ArticleToBusinessPartners.AddRange(entities: artToBusinessPartner);
            context.SaveChanges();
        }
    }
}