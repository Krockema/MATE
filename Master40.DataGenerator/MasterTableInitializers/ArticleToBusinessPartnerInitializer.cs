using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.MasterTableInitializers
{
    public class ArticleToBusinessPartnerInitializer
    {

        public void Init(MasterDBContext dbContext, M_Article[] articleTable, MasterTableBusinessPartner businessPartner)
        {
            List<M_ArticleToBusinessPartner> articleToBusinessPartners = new List<M_ArticleToBusinessPartner>();
            foreach (var article in articleTable)
            {
                articleToBusinessPartners.Add(new M_ArticleToBusinessPartner()
                {
                    BusinessPartnerId = businessPartner.KREDITOR_MATERIAL_WHOLSALE.Id,
                    ArticleId = article.Id,
                    PackSize = 10,
                    Price = 20.00,
                    TimeToDelivery = 480
                });
            }

            dbContext.ArticleToBusinessPartners.AddRange(articleToBusinessPartners);
            dbContext.SaveChanges();
        }

    }
}