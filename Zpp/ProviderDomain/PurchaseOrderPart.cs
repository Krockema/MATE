using System.Collections.Generic;
using Master40.DB.Data.WrappersForPrimitives;
using Zpp.DemandDomain;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Zpp.LotSize;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.ProviderDomain
{
    /**
     * wraps T_PurchaseOrderPart
     */
    public class PurchaseOrderPart : Provider, IProviderLogic
    {
        public PurchaseOrderPart(IProvider provider, Demands demands,
            IDbMasterDataCache dbMasterDataCache) : base(provider, dbMasterDataCache)
        {
        }

        public override IProvider ToIProvider()
        {
            return (T_PurchaseOrderPart) _provider;
        }

        public override Id GetArticleId()
        {
            Id articleId = new Id(((T_PurchaseOrderPart) _provider).ArticleId);
            return articleId;
        }

        public override Demands CreateNeededDemands(M_Article article,
            IDbTransactionData dbTransactionData, IDbMasterDataCache dbMasterDataCache,
            Provider parentProvider, Quantity quantity)
        {
            throw new System.NotImplementedException();
        }

        public static Provider CreatePurchaseOrderPart(Id demandId, M_Article article, DueTime dueTime, Quantity lotSize,
            IDbMasterDataCache dbMasterDataCache)
        {
            if (article.ToBuild)
            {
                throw new MrpRunException("You try to create a purchaseOrderPart for a articleToBuild.");
            }
            
            // currently only one businessPartner per article TODO: This could be changing
            M_ArticleToBusinessPartner articleToBusinessPartner =
                dbMasterDataCache.M_ArticleToBusinessPartnerGetAllByArticleId(article.GetId())
                    [0];
            M_BusinessPartner businessPartner =
                dbMasterDataCache.M_BusinessPartnerGetById(new Id(articleToBusinessPartner
                    .BusinessPartnerId));
            T_PurchaseOrder purchaseOrder = new T_PurchaseOrder();
            // [Name],[DueTime],[BusinessPartnerId]
            purchaseOrder.DueTime = dueTime.GetValue();
            purchaseOrder.BusinessPartner = businessPartner;
            purchaseOrder.Name = $"PurchaseOrder{article.Name} for " +
                                 $"businessPartner {purchaseOrder.BusinessPartner.Id}";


            // demand cannot be fulfilled in time
            if (articleToBusinessPartner.DueTime > dueTime.GetValue())
            {
                Logger.Error($"Article {article.GetId()} from demand {demandId} " +
                             $"should be available at {dueTime}, but " +
                             $"businessPartner {businessPartner.Id} " +
                             $"can only deliver at {articleToBusinessPartner.DueTime}.");
            }

            // init a new purchaseOderPart
            T_PurchaseOrderPart purchaseOrderPart = new T_PurchaseOrderPart();

            // [PurchaseOrderId],[ArticleId],[Quantity],[State],[ProviderId]
            purchaseOrderPart.PurchaseOrder = purchaseOrder;
            purchaseOrderPart.Article = article;
            purchaseOrderPart.ArticleId = article.Id;
            purchaseOrderPart.Quantity =
                PurchaseManagerUtils.calculateQuantity(articleToBusinessPartner,
                    lotSize) *
                articleToBusinessPartner
                    .PackSize; // TODO: is amount*packSize in var quantity correct?
            purchaseOrderPart.State = State.Created;
            // connects this provider with table T_Provider
            purchaseOrderPart.Provider = new T_Provider();
            purchaseOrderPart.ProviderId = purchaseOrderPart.Provider.Id;

            Logger.Debug("PurchaseOrderPart created.");
            return new PurchaseOrderPart(purchaseOrderPart, null, dbMasterDataCache);
        }

        public override string GetGraphizString()
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"P(PuOP);{GetQuantity()};{GetArticle().Name}";
            return graphizString;
        }
    }
}