using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.Enums;
using Master40.DB.Interfaces;

namespace Zpp
{
    public class PurchaseManager
    {
        private readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private readonly Dictionary<int, T_PurchaseOrder> _purchaseOrders =
            new Dictionary<int, T_PurchaseOrder>();

        private int counter = 0;
        private readonly ProductionDomainContext _productionDomainContext;
        private readonly List<IProvider> _providers;
        private readonly List<T_DemandToProvider> _demandToProvider;

        public PurchaseManager(ProductionDomainContext productionDomainContext,
            List<IProvider> providers, List<T_DemandToProvider> demandToProviders)
        {
            _productionDomainContext = productionDomainContext;
            _providers = providers;
            _demandToProvider = demandToProviders;
            initPurchaseOrders();
        }

        public void createPurchaseOrderPart(IDemand demand)
        {
            // currently only one business per article
            M_ArticleToBusinessPartner articleToBusinessPartner = demand.GetArticle()
                .ArticleToBusinessPartners
                .OfType<M_ArticleToBusinessPartner>()
                .First();
            T_PurchaseOrder purchaseOrder =
                _purchaseOrders[articleToBusinessPartner.BusinessPartnerId];
            if (purchaseOrder.DueTime == 0)
            {
                purchaseOrder.DueTime = demand.GetDueTime();
            }

            // demand cannot be fulfilled in time
            if (articleToBusinessPartner.DueTime > demand.GetDueTime())
            {
                LOGGER.Error(
                    $"Article {demand.GetArticle().Id} from demand {demand.Id} should be available at {demand.GetDueTime()}, but businessPartner {articleToBusinessPartner.BusinessPartner.Id} can only deliver at {articleToBusinessPartner.DueTime}.");
            }

            // close purchaseOrder if given purchaseOrderPosition is out of time
            if (articleToBusinessPartner.DueTime > purchaseOrder.DueTime)
            {
                closeOpenPurchaseOrder(articleToBusinessPartner.BusinessPartner);
            }

            T_PurchaseOrderPart purchaseOrderPart = new T_PurchaseOrderPart();
            purchaseOrder.PurchaseOrderParts.Add(purchaseOrderPart);

            purchaseOrderPart.PurchaseOrder = purchaseOrder;
            purchaseOrderPart.Article = demand.GetArticle();
            purchaseOrderPart.Quantity =
                calculateQuantity(articleToBusinessPartner, demand.GetQuantity());
            purchaseOrderPart.State = State.Created;
            purchaseOrderPart.Provider = new T_Provider();


            LOGGER.Debug("PurchaseOrderPart created.");
        }

        /// <summary>
        /// State Start: empty _purchaseOrder, list with created _purchaseOrderParts
        /// Transition: add list _purchaseOrderParts to _purchaseOrder
        /// State End: list _purchaseOrders is extended by created purchaseOrder, list _purchaseOrderParts & _purchaseOrder is reset
        /// </summary>
        /// <param name="name"></param>
        /// <param name="businessPartner"></param>
        private void createPurchaseOrder(string name, M_BusinessPartner businessPartner)
        {
            T_PurchaseOrder purchaseOrder =
                _purchaseOrders[businessPartner.Id];
            _productionDomainContext.PurchaseOrders.Add(purchaseOrder);

            // fill _purchaseOrder
            purchaseOrder.Name = name;
            purchaseOrder.BusinessPartner = businessPartner;

            // reset
            initPurchaseOrder(businessPartner);

            LOGGER.Debug($"PurchaseOrder {purchaseOrder.Name} created.");
        }

        private void initPurchaseOrder(M_BusinessPartner businessPartner)
        {
            _purchaseOrders[businessPartner.Id] = new T_PurchaseOrder();
            _purchaseOrders[businessPartner.Id].PurchaseOrderParts =
                new List<T_PurchaseOrderPart>();
        }

        private void initPurchaseOrders()
        {
            foreach (M_BusinessPartner businessPartner in _productionDomainContext.BusinessPartners)
            {
                initPurchaseOrder(businessPartner);
            }
        }

        private void closeOpenPurchaseOrder(M_BusinessPartner businessPartner)
        {
            createPurchaseOrder($"PurchaseOrder{counter}", businessPartner);
            counter++;
        }

        public void closeOpenPurchaseOrders()
        {
            foreach (M_BusinessPartner businessPartner in _productionDomainContext.BusinessPartners)
            {
                closeOpenPurchaseOrder(businessPartner);
            }
        }

        private int calculateQuantity(M_ArticleToBusinessPartner articleToBusinessPartner,
            decimal demandQuantity)
        {
            int purchaseQuantity = 0;
            // ATTENTION: <= since cast from decimal to integer could be round down
            for (int quantity = 0;
                quantity <= demandQuantity;
                quantity += articleToBusinessPartner.PackSize)
            {
                purchaseQuantity++;
            }

            return purchaseQuantity;
        }
    }
}