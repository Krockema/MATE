using System.Collections.Generic;
using System.Linq;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Zpp
{
    /// <summary>
    /// This is a simple implementation that creates a purchaseOrder per demand
    /// instead of grouping all demands within a certain dueTime
    /// </summary>
    public class PurchaseManagerSimple : IPurchaseManager
    {
        private readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private readonly IDbCache _dbCache;
        private readonly IProviderManager _providerManager;

        public PurchaseManagerSimple(IDbCache dbCache, IProviderManager providerManager)
        {
            _dbCache = dbCache;
            _providerManager = providerManager;
        }

        public void createPurchaseOrderPart(IDemand demand)
        {
            // currently only one businessPartner per article
            M_ArticleToBusinessPartner articleToBusinessPartner = demand.GetArticle()
                .ArticleToBusinessPartners.OfType<M_ArticleToBusinessPartner>().First();
            T_PurchaseOrder purchaseOrder = new T_PurchaseOrder();
            // [Name],[DueTime],[BusinessPartnerId]
            purchaseOrder.DueTime = demand.GetDueTime();
            purchaseOrder.BusinessPartner = articleToBusinessPartner.BusinessPartner;
            purchaseOrder.Name = $"PurchaseOrder{demand.GetArticle().Name} for " +
                                 $"businessPartner {purchaseOrder.BusinessPartner.Id}";


            // demand cannot be fulfilled in time
            if (articleToBusinessPartner.DueTime > demand.GetDueTime())
            {
                LOGGER.Error($"Article {demand.GetArticle().Id} from demand {demand.Id} " +
                             $"should be available at {demand.GetDueTime()}, but " +
                             $"businessPartner {articleToBusinessPartner.BusinessPartner.Id} " +
                             $"can only deliver at {articleToBusinessPartner.DueTime}.");
            }

            // init a new purchaseOderPart
            T_PurchaseOrderPart purchaseOrderPart = new T_PurchaseOrderPart();
            _providerManager.AddProvider(purchaseOrderPart);

            // [PurchaseOrderId],[ArticleId],[Quantity],[State],[ProviderId]
            purchaseOrderPart.PurchaseOrder = purchaseOrder;
            purchaseOrderPart.Article = demand.GetArticle();
            purchaseOrderPart.Quantity =
                PurchaseManagerUtils.calculateQuantity(articleToBusinessPartner,
                    demand.GetQuantity());
            purchaseOrderPart.State = State.Created;
            // connects this provider with table T_Provider
            purchaseOrderPart.Provider = new T_Provider();


            LOGGER.Debug("PurchaseOrderPart created.");
        }
    }
}