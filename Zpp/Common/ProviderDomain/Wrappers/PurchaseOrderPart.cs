using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.Common.DemandDomain.WrappersForCollections;
using Zpp.DbCache;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.ProviderDomain.Wrappers
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

        public override void CreateDependingDemands(M_Article article,
            IDbTransactionData dbTransactionData, Provider parentProvider, Quantity demandedQuantity)
        {
            throw new System.NotImplementedException();
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"P(PuOP);{base.GetGraphizString(dbTransactionData)}";
            return graphizString;
        }

        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            T_PurchaseOrderPart purchaseOrderPart = ((T_PurchaseOrderPart) _provider);
            if (purchaseOrderPart.PurchaseOrder == null)
            {
                purchaseOrderPart.PurchaseOrder =
                    dbTransactionData.PurchaseOrderGetById(
                        new Id(purchaseOrderPart.PurchaseOrderId));
            }

            return new DueTime(purchaseOrderPart.PurchaseOrder.DueTime);
        }

        public override DueTime GetStartTime(IDbTransactionData dbTransactionData)
        {
            // currently only one businessPartner per article TODO: This could be changing
            M_ArticleToBusinessPartner articleToBusinessPartner =
                _dbMasterDataCache.M_ArticleToBusinessPartnerGetAllByArticleId(GetArticleId())[0];
            return GetDueTime(dbTransactionData).Minus(articleToBusinessPartner.DueTime);
        }

        public override void SetDueTime(DueTime newDueTime, IDbTransactionData dbTransactionData)
        {
            T_PurchaseOrderPart purchaseOrderPart = ((T_PurchaseOrderPart) _provider);
            if (purchaseOrderPart.PurchaseOrder == null)
            {
                purchaseOrderPart.PurchaseOrder =
                    dbTransactionData.PurchaseOrderGetById(
                        new Id(purchaseOrderPart.PurchaseOrderId));
            }

            purchaseOrderPart.PurchaseOrder.DueTime = newDueTime.GetValue();
        }

        public override void SetProvided(DueTime atTime)
        {
            throw new System.NotImplementedException();
        }
    }
}