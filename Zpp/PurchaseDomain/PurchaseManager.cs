using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
using Zpp.Utils;
using Zpp.WrappersForPrimitives;

namespace Zpp.PurchaseDomain
{
    public class PurchaseManager : IProvidingManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDbMasterDataCache _dbMasterDataCache;

        public PurchaseManager(IDbMasterDataCache dbMasterDataCache)
        {
            _dbMasterDataCache = dbMasterDataCache;
        }

        public Response Satisfy(Demand demand, Quantity demandedQuantity, IDbTransactionData dbTransactionData)
        {
            M_Article article = demand.GetArticle();
            DueTime dueTime = demand.GetDueTime(dbTransactionData);
            if (article.ToBuild)
            {
                throw new MrpRunException(
                    "You try to create a purchaseOrderPart for a articleToBuild.");
            }

            // currently only one businessPartner per article TODO: This could be changing
            M_ArticleToBusinessPartner articleToBusinessPartner =
                _dbMasterDataCache.M_ArticleToBusinessPartnerGetAllByArticleId(article.GetId())[0];
            M_BusinessPartner businessPartner =
                _dbMasterDataCache.M_BusinessPartnerGetById(new Id(articleToBusinessPartner
                    .BusinessPartnerId));
            T_PurchaseOrder purchaseOrder = new T_PurchaseOrder();
            // [Name],[DueTime],[BusinessPartnerId]
            purchaseOrder.DueTime = dueTime.GetValue() - articleToBusinessPartner.DueTime;
            purchaseOrder.BusinessPartner = businessPartner;
            purchaseOrder.Name = $"PurchaseOrder{article.Name} for " +
                                 $"businessPartner {purchaseOrder.BusinessPartner.Id}";

            // demand cannot be fulfilled in time
            if (articleToBusinessPartner.DueTime > dueTime.GetValue())
            {
                Logger.Error($"Article {article.GetId()} from demand {demand.GetId()} " +
                             $"should be available at {dueTime}, but " +
                             $"businessPartner {businessPartner.Id} " +
                             $"can only deliver at {articleToBusinessPartner.DueTime}.");
            }

            // init a new purchaseOderPart
            T_PurchaseOrderPart tPurchaseOrderPart = new T_PurchaseOrderPart();

            // [PurchaseOrderId],[ArticleId],[Quantity],[State],[ProviderId]
            tPurchaseOrderPart.PurchaseOrder = purchaseOrder;
            tPurchaseOrderPart.Article = article;
            tPurchaseOrderPart.ArticleId = article.Id;
            tPurchaseOrderPart.Quantity =
                PurchaseManagerUtils.calculateQuantity(articleToBusinessPartner, demandedQuantity) *
                articleToBusinessPartner
                    .PackSize;
            if (tPurchaseOrderPart.Quantity < demandedQuantity.GetValue())
            {
                throw new MrpRunException("You cannot purchase less than you need!");
            }

            tPurchaseOrderPart.State = State.Created;

            Logger.Debug("PurchaseOrderPart created.");
            PurchaseOrderPart purchaseOrderPart =
                new PurchaseOrderPart(tPurchaseOrderPart, null, _dbMasterDataCache);
            
            T_DemandToProvider demandToProvider = new T_DemandToProvider()
            {
                DemandId = demand.GetId().GetValue(),
                ProviderId = purchaseOrderPart.GetId().GetValue(),
                Quantity = demandedQuantity.GetValue()
            };
            
            return new Response(purchaseOrderPart, demandToProvider, demandedQuantity);
        }
    }
}