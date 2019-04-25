using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.DataModel;
using Master40.DB.ReportingModel;

namespace Master40.DB.Data.Context
{
    public class ProductionDomainContext : MasterDBContext
    {
        public ProductionDomainContext(DbContextOptions<MasterDBContext> options) : base(options) { }
        
        public T_CustomerOrder OrderById(int id)
        {
            return CustomerOrders.FirstOrDefault(x => x.Id == id);
        }
        /// <summary>
        /// Receives the prior items to a given ProductionOrderWorkSchedule
        /// </summary>
        /// <param name="productionOrderWorkSchedule"></param>
        /// <returns>List<ProductionOrderWorkSchedule></returns>
        public Task<List<T_ProductionOrderOperation>> GetFollowerProductionOrderWorkSchedules(T_ProductionOrderOperation productionOrderWorkSchedule)
        {
            var rs = Task.Run(() =>
            {
                var priorItems = new List<T_ProductionOrderOperation>();
                // If == min Hierarchy --> get Pevious Article -> Highest Hierarchy Workschedule Item
                var maxHierarchy = ProductionOrderOperations.Where(x => x.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId)
                                                    .Max(x => x.HierarchyNumber);

                if (maxHierarchy == productionOrderWorkSchedule.HierarchyNumber)
                {
                    // get Previous Article

                    priorItems.AddRange(GetParents(productionOrderWorkSchedule));
                }
                else
                {
                    // get Previous Workschedule
                    var previousPows =
                        ProductionOrderOperations.Where(
                                x => x.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId
                                     && x.HierarchyNumber > productionOrderWorkSchedule.HierarchyNumber)
                            .OrderBy(x => x.HierarchyNumber).FirstOrDefault();
                    priorItems.Add(previousPows);
                }
                return priorItems;
            });

            return rs;
        }

        public Task<List<SimulationWorkschedule>> GetFollowerProductionOrderWorkSchedules(SimulationWorkschedule simulationWorkSchedule, SimulationType type, List<SimulationWorkschedule> relevantItems)
        {
            var rs = Task.Run(() =>
            {
                var priorItems = new List<SimulationWorkschedule>();
                // If == min Hierarchy --> get Pevious Article -> Highest Hierarchy Workschedule Item
                var maxHierarchy = relevantItems.Where(x => x.ProductionOrderId == simulationWorkSchedule.ProductionOrderId)
                    .Max(x => x.HierarchyNumber);

                if (maxHierarchy == simulationWorkSchedule.HierarchyNumber)
                {
                    // get Previous Article
                    priorItems.AddRange(relevantItems
                        .Where(x => x.ProductionOrderId == simulationWorkSchedule.ParentId
                                && x.HierarchyNumber == relevantItems
                                    .Where(w => w.ProductionOrderId == simulationWorkSchedule.ParentId)
                                    .Min(m => m.HierarchyNumber)));
                }
                else
                {
                    // get Previous Workschedule
                    var previousPows =
                        relevantItems.Where(
                                x => x.ProductionOrderId == simulationWorkSchedule.ProductionOrderId
                                     && x.HierarchyNumber > simulationWorkSchedule.HierarchyNumber)
                            .OrderBy(x => x.HierarchyNumber).FirstOrDefault();
                    priorItems.Add(previousPows);
                }
                return priorItems;
            });
            return rs;
        }

        public List<T_ProductionOrderOperation> GetParents(T_ProductionOrderOperation schedule)
        {
            var parents = new List<T_ProductionOrderOperation>();
            if (schedule == null) return parents;
            var parent = GetHierarchyParent(schedule);
            if (parent != null)
            {
                parents.Add(parent);
                return parents;
            }
            var bomParents = GetBomParents(schedule);
            return bomParents ?? parents;
        }

        public T_ProductionOrderOperation GetHierarchyParent(T_ProductionOrderOperation pows)
        {

            T_ProductionOrderOperation hierarchyParent = null;
            var hierarchyParentNumber = int.MaxValue;
            //find next higher element
            foreach (var mainSchedule in pows.ProductionOrder.ProductionOrderWorkSchedule)
            {
                //if (mainSchedule.ProductionOrderId != pows.ProductionOrderId) continue;
                if (mainSchedule.HierarchyNumber <= pows.HierarchyNumber ||
                    mainSchedule.HierarchyNumber >= hierarchyParentNumber) continue;
                hierarchyParent = mainSchedule;
                hierarchyParentNumber = mainSchedule.HierarchyNumber;
            }
            return hierarchyParent;
        }

        public List<T_ProductionOrderOperation> GetBomParents(T_ProductionOrderOperation plannedSchedule)
        {
            var provider = plannedSchedule.ProductionOrder.DemandProviderProductionOrders;
            if (provider == null || provider.ToList().Any(dppo => dppo.DemandRequester == null))
                return new List<T_ProductionOrderOperation>();
            var requester =  (from demandProviderProductionOrder in provider
                    select demandProviderProductionOrder.DemandRequester into req
                    select req).ToList();
            

            var pows = new List<T_ProductionOrderOperation>();
            foreach (var singleRequester in requester)
            {
                if (singleRequester.GetType() == typeof(DemandCustomerOrderPart) || singleRequester.GetType() == typeof(DemandStock)) return null;
                var demand = DemandToProviders.OfType<DemandProductionOrderBom>().Include(a => a.ProductionOrderBom)
                    .ThenInclude(b => b.ProductionOrderParent).ThenInclude(c => c.ProductionOrderWorkSchedule)
                    .Single(a => a.Id == singleRequester.Id);
                var schedules = demand.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule;
                pows.Add(schedules.Single(a => a.HierarchyNumber == schedules.Min(b => b.HierarchyNumber)));
            }
            return pows;

            /*return (from demandProviderProductionOrder in provider
            select demandProviderProductionOrder.DemandRequester into requester
            where requester.GetType() == typeof(DemandProductionOrderBom)
            select ((DemandProductionOrderBom)requester).ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule into schedules
            select schedules.Single(a => a.HierarchyNumber == schedules.Min(b => b.HierarchyNumber))).ToList();*/
        }



        public async Task<M_Article> GetArticleBomRecursive(M_Article article, int articleId)
        {
            article.ArticleChilds = ArticleBoms.Include(a => a.ArticleChild)
                                                        .ThenInclude(w => w.WorkSchedules)
                                                        .Where(a => a.ArticleParentId == articleId).ToList();

            foreach (var item in article.ArticleChilds)
            {
                await GetArticleBomRecursive(item.ArticleParent, item.ArticleChildId);
            }
            await Task.Yield();
            return article;

        }

        /// <summary>
        /// returns the Production Order Work Schedules for a given Order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Task<List<T_ProductionOrderOperation>> GetProductionOrderWorkScheduleByOrderId(int orderId)
            {
                return Task.Run(() =>
                {
                    // get the corresponding Order Parts to Order
                    var demands = DemandToProviders.OfType<DemandCustomerOrderPart>()
                        .Include(x => x.OrderPart)
                        .Where(o => o.OrderPart.OrderId == orderId)
                        .ToList();

                    // ReSharper Linq
                    var demandboms = demands.SelectMany(demand => DemandToProviders.OfType<DemandProductionOrderBom>()
                        .Where(a => a.DemandRequesterId == demand.Id)).ToList();

                    // get Demand Providers for this Order
                    var demandProviders = new List<ProviderProductionOrder>();
                    foreach (var order in (DemandToProviders.OfType<ProviderProductionOrder>()
                        .Join(demands, c => c.DemandRequesterId, d => ((IDemandToProvider)d).Id, (c, d) => c)))
                    {
                        demandProviders.Add(order);
                    }

                    var demandBomProviders = (DemandToProviders.OfType<ProviderProductionOrder>()
                        .Join(demandboms, c => c.DemandRequesterId, d => d.Id, (c, d) => c)).ToList();

                    // get ProductionOrderWorkSchedule for 
                    var pows = ProductionOrderOperations.Include(x => x.ProductionOrder).Include(x => x.Machine).Include(x => x.MachineGroup).AsNoTracking();
                    var powDetails = pows.Join(demandProviders, p => p.ProductionOrderId, dp => dp.ProductionOrderId,
                        (p, dp) => p).ToList();

                    var powBoms = (from p in pows
                                   join dbp in demandBomProviders on p.ProductionOrderId equals dbp.ProductionOrderId
                                   select p).ToList();

                    powDetails.AddRange(powBoms);
                    return powDetails;
                });
            }

            /// <summary>
            /// returns the OrderIds for the ProductionOrder
            /// </summary>
            /// <param name="po"></param>
            /// <returns></returns>
            public List<int> GetOrderIdsFromProductionOrder(T_ProductionOrder po)
            {
                po = ProductionOrders.Include(a => a.DemandProviderProductionOrders).ThenInclude(b => b.DemandRequester).Single(a => a.Id == po.Id);
                var ids = new List<int>();
                var requester = (from provider in po.DemandProviderProductionOrders
                    select provider.DemandRequester).ToList();
                if (!requester.Any() || requester.First() == null) return ids;
                foreach (var singleRequester in requester)
                {
                    if (singleRequester.GetType() == typeof(DemandProductionOrderBom))
                    {

                        ids.AddRange(GetOrderIdsFromProductionOrder(
                            ((DemandProductionOrderBom) singleRequester).ProductionOrderBom.ProductionOrderParent));
                    }
                    else if (singleRequester.GetType() == typeof(DemandCustomerOrderPart))
                    {
                        var dop = DemandToProviders.OfType<DemandCustomerOrderPart>().Include(a => a.OrderPart).Single(a => a.Id == singleRequester.Id);
                        ids.Add(dop.OrderPart.OrderId);
                    }

                }
                return ids;
            }

            /// <summary>
            /// Deepseach thorugh tree to Return all Workschedules Related to one Order.
            /// </summary>
            /// <param name="demandRequester"></param>
            /// <param name="productionOrderWorkSchedule"></param>
            /// <returns>List of ProductionOrderWorkSchedules - Attention May Include Dupes Through Complex Backlinks !</returns>
            public List<T_ProductionOrderOperation> GetWorkSchedulesFromDemand(IDemandToProvider demandRequester, ref List<T_ProductionOrderOperation> productionOrderWorkSchedule)
            {
                foreach (var item in demandRequester.DemandProvider.OfType<ProviderProductionOrder>())
                {
                    var productionOrders = ProductionOrders
                        .Include(x => x.ProductionOrderWorkSchedule)
                            .ThenInclude(x => x.MachineGroup)
                        .Include(x => x.ProductionOrderWorkSchedule)
                            .ThenInclude(x => x.Machine)
                        .Include(x => x.ProductionOrderBoms)
                            .ThenInclude(x => x.DemandProductionOrderBoms)
                                .ThenInclude(x => x.DemandProvider)
                        .FirstOrDefault(x => x.Id == item.ProductionOrderId);

                    productionOrderWorkSchedule.AddRange(productionOrders.ProductionOrderWorkSchedule);
                    foreach (var po in productionOrders.ProductionOrderBoms)
                    {
                        foreach (var dpob in po.DemandProductionOrderBoms)
                        {
                            GetWorkSchedulesFromDemand(dpob, ref productionOrderWorkSchedule);
                        }
                    }
                }
                return productionOrderWorkSchedule;
            }



            /***
             * Requirements
             * */
            /*
           /// <summary>
           /// Creates stock reservation if possible
           /// </summary>
           /// <param name="stock"></param>
           /// <param name="demand"></param>
           public void TryCreateStockReservation(Stock stock, IDemandToProvider demand)
           {
               var stockReservations = GetReserved(demand.ArticleId);
               var bought = GetAmountBought(demand.ArticleId);
               //get the current amount of free available articles
               var current = stock.Current + bought - stockReservations;
               decimal quantity;
               //either reserve all that are in stock or the amount needed
               quantity = demand.Quantity > current ? current : demand.Quantity;
               if (quantity == 0) return;
               var demandProviderStock = new DemandProviderStock()
               {
                   ArticleId = stock.ArticleForeignKey,
                   Quantity = quantity,
                   DemandRequesterId = demand.DemandRequesterId ?? demand.Id,
                   StockId = stock.Id
               };
               Demands.Add(demandProviderStock);
               SaveChanges();
           }*/

        public List<T_ProductionOrder> CreateChildProductionOrders(IDemandToProvider demand, decimal amount, SimulationConfiguration simConfig)
        {
            T_ProductionOrderBom bom = null;
            if (demand.GetType() == typeof(DemandProductionOrderBom))
            {
                bom = ProductionOrderBoms.FirstOrDefault(a => a.Id == ((DemandProductionOrderBom) demand).ProductionOrderBomId);
            }
            
            var lotsize = simConfig.Lotsize;
            var productionOrders = new List<T_ProductionOrder>();
            /*decimal bomQuantity;
            if (bom != null)
                bomQuantity = ArticleBoms.AsNoTracking().Single(a => a.ArticleChildId == demand.ArticleId &&
                    a.ArticleParentId == bom.ProductionOrderParent.ArticleId).Quantity * lotsize;
            else 
                bomQuantity = ArticleBoms.AsNoTracking().Single(a =>a.ArticleChildId == demand.ArticleId && 
                    a.ArticleParentId == null).Quantity * lotsize;
            */
            while (amount > 0)// || bomQuantity > 0)
            {
                var productionOrder = CreateProductionOrder(demand,GetDueTimeByOrder(demand),simConfig);
                if (amount > 0)
                {
                    var demandProviderProductionOrder = CreateDemandProviderProductionOrder(demand, productionOrder,
                        amount > lotsize ? lotsize : amount);

                    //if the article has a parent create a relationship
                    if (bom != null)
                    {
                        demandProviderProductionOrder.DemandRequesterId = demand.Id;
                        DemandToProviders.Update(demandProviderProductionOrder);
                    }
                }
                SaveChanges();
                amount -= lotsize;
                //bomQuantity -= lotsize;
                productionOrders.Add(productionOrder);
            }
            
            return productionOrders;
        }

        public DemandStock CreateStockDemand(IDemandToProvider demand, M_Stock stock, decimal amount)
        {
            var demandStock = new DemandStock()
            {
                Quantity = amount,
                Article = demand.Article,
                ArticleId = demand.ArticleId,
                State = State.Created,
                DemandProvider = new List<T_Demand>(),
                StockId = stock.Id,
            };
            DemandToProviders.Add(demandStock);
            SaveChanges();
            return demandStock;
        }

        public T_ProductionOrder CreateProductionOrder(IDemandToProvider demand, int duetime, SimulationConfiguration simulationConfiguration)
        {
            var productionOrder = new T_ProductionOrder()
            {
                ArticleId = demand.Article.Id,
                Quantity = simulationConfiguration.Lotsize,
                Duetime = duetime
            };
            
            ProductionOrders.Add(productionOrder);
            SaveChanges();
            return productionOrder;
        }

        public decimal GetPlannedStock(M_Stock stock, IDemandToProvider demand)
        {
            var amountReserved = GetReserved(demand.ArticleId);
            var amountBought = 0;
            var articlesBought = PurchaseOrderParts.Where(a => a.ArticleId == demand.ArticleId && a.State != State.Finished);
            foreach (var articleBought in articlesBought)
            {
                amountBought += articleBought.Quantity;
            }
            //just produced articles have a reason and parents they got produced for so they cannot be reserved by another requester
            var amountJustProduced = DemandToProviders.OfType<DemandProductionOrderBom>()
                .Where(a => (a.State != State.Finished || a.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule.All(b => b.ProducingState == ProducingState.Created || b.ProducingState == ProducingState.Waiting))
                            && a.ArticleId == demand.ArticleId
                            && a.DemandProvider.Any()
                            && a.DemandProvider.All(b => b.State == State.Finished)).Sum(a => a.Quantity);
            //plannedStock is the amount of this article in stock after taking out the amount needed
            var plannedStock = stock.Current + amountBought - demand.Quantity - amountReserved - amountJustProduced;

            return plannedStock;
        }

        public ProviderProductionOrder CreateDemandProviderProductionOrder(IDemandToProvider demand, T_ProductionOrder productionOrder, decimal amount)
        {
            var demandProviderProductionOrder = new ProviderProductionOrder()
            {
                Quantity = amount,
                Article = demand.Article,
                ArticleId = demand.ArticleId,
                ProductionOrderId = productionOrder.Id,
                DemandRequesterId = demand.Id
            };
            DemandToProviders.Add(demandProviderProductionOrder);
            if (productionOrder.DemandProviderProductionOrders == null) productionOrder.DemandProviderProductionOrders = new List<ProviderProductionOrder>();
            productionOrder.DemandProviderProductionOrders.Add(demandProviderProductionOrder);
            SaveChanges();

            return demandProviderProductionOrder;
        }

        //Todo: check logic
        public void CreatePurchaseDemand(IDemandToProvider demand, decimal amount, int time)
        {
            if (NeedToRefill(demand, amount))
            {
                var providerPurchasePart = new ProviderPurchasePart()
                {
                    Quantity = amount,
                    ArticleId = demand.ArticleId,
                    DemandRequesterId = demand.Id,
                    State = State.Created,
                };
                DemandToProviders.Add(providerPurchasePart);

                CreatePurchase(demand, amount, providerPurchasePart,time);
                DemandToProviders.Update(providerPurchasePart);
            }
            else
            {
                var providerStock = new ProviderStock()
                {
                    Quantity = amount,
                    ArticleId = demand.ArticleId,
                    DemandRequesterId = demand.Id,
                    State = State.Created,
                    StockId = Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Id
                };
                DemandToProviders.Add(providerStock);
            }
            SaveChanges();
        }

        public void CreatePurchase(IDemandToProvider demand, decimal amount, ProviderPurchasePart providerPurchasePart, int time)
        {
            var articleToPurchase = ArticleToBusinessPartners.Single(a => a.ArticleId == demand.ArticleId);
            var purchase = new T_PurchaseOrder()
            {
                BusinessPartnerId = articleToPurchase.BusinessPartnerId,
                DueTime = articleToPurchase.DueTime + time
            };
            //amount to be purchased has to be raised to fit the packsize
            amount = Math.Floor(amount / articleToPurchase.PackSize) * articleToPurchase.PackSize;
            var purchasePart = new T_PurchaseOrderPart()
            {
                ArticleId = demand.ArticleId,
                Quantity = (int)amount,
                DemandProviderPurchaseParts = new List<ProviderPurchasePart>() { providerPurchasePart },
                PurchaseId = purchase.Id
            };
            purchase.PurchaseParts = new List<T_PurchaseOrderPart>()
            {
                purchasePart
            };

            PurchaseOrders.Add(purchase);
            PurchaseOrderParts.Add(purchasePart);
            SaveChanges();
        }

        public bool NeedToRefill(IDemandToProvider demand, decimal amount)
        {
            var purchasedAmount = GetAmountBought(demand.ArticleId);
            var neededAmount = GetReserved(demand.ArticleId);
            var stockMin = Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Min;
            return (purchasedAmount - neededAmount - amount < stockMin);
        }

        public ProviderStock CreateDemandProviderStock(IDemandToProvider demand, decimal amount)
        {
            var dps = new ProviderStock()
            {
                ArticleId = demand.ArticleId,
                Quantity = amount,
                StockId = Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Id,
                DemandRequesterId = demand.Id,
                State = State.Created
            };
            Add(dps);
            SaveChanges();
            return dps;
        }
        public ProviderProductionOrder CreateProviderProductionOrder(IDemandToProvider demand, T_ProductionOrder productionOrder, decimal amount)
        {
            var dppo = new ProviderProductionOrder()
            {
                ArticleId = demand.ArticleId,
                Quantity = amount,
                ProductionOrderId = productionOrder.Id,
                DemandRequesterId = demand.DemandRequesterId,
            };
            Add(dppo);
            SaveChanges();
            return dppo;
        }
        public ProviderPurchasePart CreateDemandProviderPurchasePart(IDemandToProvider demand, T_PurchaseOrderPart purchase, decimal amount)
        {
            var dppp = new ProviderPurchasePart()
            {
                ArticleId = demand.ArticleId,
                Quantity = amount,
                PurchasePartId = purchase.Id,
                DemandRequesterId = demand.Id,
            };
            Add(dppp);
            SaveChanges();
            return dppp;
        }


        public void AssignDemandProviderToProductionOrderBom(DemandProductionOrderBom demand, T_ProductionOrderBom pob)
        {
            if (pob.DemandProductionOrderBoms == null)
                pob.DemandProductionOrderBoms = new List<DemandProductionOrderBom>();
            pob.DemandProductionOrderBoms.Add(demand);
            demand.ProductionOrderBomId = pob.Id;
            // Update(demand);
            // Update(pob);            
        }

        public void AssignProviderToDemand(IDemandToProvider demand, T_Demand provider)
        {
            if (demand.DemandProvider == null) demand.DemandProvider = new List<T_Demand>();
            demand.DemandProvider.Add(provider);
            Update(demand);
            SaveChanges();
        }

        public T_ProductionOrderBom TryCreateProductionOrderBoms(IDemandToProvider demand, T_ProductionOrder parentProductionOrder, SimulationConfiguration simConfig)
        {
            if (parentProductionOrder == null) return null;
            var lotsize = simConfig.Lotsize;
            var quantity = demand.Quantity > lotsize ? lotsize : demand.Quantity;
            var pob = new T_ProductionOrderBom()
            {
                Quantity = quantity,
                ProductionOrderParentId = parentProductionOrder.Id
            };
            Add(pob);
            if (demand.GetType() == typeof(DemandProductionOrderBom))
                AssignDemandProviderToProductionOrderBom((DemandProductionOrderBom)demand, pob);
            SaveChanges();
            return pob;
        }
        
        public void AssignPurchaseToDemandProvider(T_PurchaseOrderPart purchasePart, ProviderPurchasePart provider, int quantity)
        {
            provider.PurchasePartId = purchasePart.Id;
            provider.Quantity = quantity;
            Update(provider);
            SaveChanges();
        }

       
        public T_PurchaseOrderPart CreatePurchase(IDemandToProvider demand, decimal amount)
        {
            var articleToPurchase = ArticleToBusinessPartners.Single(a => a.ArticleId == demand.ArticleId);
            var purchase = new T_PurchaseOrder()
            {
                BusinessPartnerId = articleToPurchase.BusinessPartnerId,
                DueTime = articleToPurchase.DueTime,
                
            };
            //amount to be purchased has to be raised to fit the packsize
            amount = Math.Ceiling(amount / articleToPurchase.PackSize) * articleToPurchase.PackSize;
            var purchasePart = new T_PurchaseOrderPart()
            {
                ArticleId = demand.ArticleId,
                Quantity = (int)amount,
                DemandProviderPurchaseParts = new List<ProviderPurchasePart>(),
                PurchaseId = purchase.Id,
                
            };
            purchase.PurchaseParts = new List<T_PurchaseOrderPart>()
            {
                purchasePart
            };
            PurchaseOrders.Add(purchase);
            PurchaseOrderParts.Add(purchasePart);
            SaveChanges();
            return purchasePart;
        }

        public int GetAvailableAmountFromProductionOrder(T_ProductionOrder productionOrder)
        {
            if (productionOrder.DemandProviderProductionOrders == null) return (int) productionOrder.Quantity;
            return (int)productionOrder.Quantity - productionOrder.DemandProviderProductionOrders.Sum(provider => (int)provider.Quantity);
        }

        public T_ProductionOrder GetEarliestProductionOrder(List<T_ProductionOrder> productionOrders)
        {
            T_ProductionOrder earliestProductionOrder = null;
            foreach (var productionOrder in productionOrders)
            {
                var po = ProductionOrders.Include(a => a.ProductionOrderWorkSchedule).Single(a => a.Id == productionOrder.Id);
                if (earliestProductionOrder == null ||
                    po.ProductionOrderWorkSchedule.Min(a => a.Start) <
                    earliestProductionOrder.ProductionOrderWorkSchedule.Min(a => a.Start))
                    earliestProductionOrder = po;
            }
            return earliestProductionOrder;
        }

        public ProviderStock TryCreateStockReservation(IDemandToProvider demand)
        {
            var stock = Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId);
            var stockReservations = GetReserved(demand.ArticleId);
            var bought = GetAmountBought(demand.ArticleId);
            var justProduced = DemandToProviders.OfType<DemandProductionOrderBom>()
                .Where(a => (a.State != State.Finished || a.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule.All(b => b.ProducingState == ProducingState.Created || b.ProducingState == ProducingState.Waiting))
                            && a.ArticleId == demand.ArticleId
                            && a.DemandProvider.Any()
                            && a.DemandProvider.All(b => b.State == State.Finished)).Sum(a => a.Quantity);
            
            //get the current amount of free available articles
            var current = stock.Current + bought - stockReservations - justProduced;
            decimal quantity;
            //either reserve all that are in stock or the amount needed
            quantity = demand.Quantity > current ? current : demand.Quantity;
            
            return quantity <= 0 ? null : CreateDemandProviderStock(demand, quantity);
        }

        public decimal GetAmountBought(int articleId)
        {
            var purchaseParts = PurchaseOrderParts.Where(a => a.ArticleId == articleId && a.State != State.Finished);
            var purchasedAmount = 0;
            foreach (var purchasePart in purchaseParts)
                purchasedAmount += purchasePart.Quantity;
            return purchasedAmount;
        }

        public decimal GetReserved(int articleId)
        {
            var demands = DemandToProviders.OfType<ProviderStock>()
                .Where(a => a.State != State.Finished && a.ArticleId == articleId).Sum(a => a.Quantity);
            return demands;
        }

        public DemandProductionOrderBom CreateDemandProductionOrderBom(int articleId, decimal quantity)
        {
            var dpob = new DemandProductionOrderBom()
            {
                Quantity = quantity,
                ArticleId = articleId,
                State = State.Created,
            };
            DemandToProviders.Add(dpob);
            SaveChanges();
            return dpob;
        }

        public void AssignProductionOrderToDemandProvider(T_ProductionOrder productionOrder, ProviderProductionOrder provider)
        {
            if (productionOrder.DemandProviderProductionOrders == null) productionOrder.DemandProviderProductionOrders = new List<ProviderProductionOrder>();
            if (!productionOrder.DemandProviderProductionOrders.Contains(provider))
                productionOrder.DemandProviderProductionOrders.Add(provider);
            Update(provider);
            SaveChanges();
        }

        public bool HasChildren(IDemandToProvider demand)
        {
            return ArticleBoms.Any(a => a.ArticleParentId == demand.ArticleId);
        }
        
        public List<T_ProductionOrder> CheckForProductionOrders(IDemandToProvider demand, decimal amount, int timer)
        {
            var possibleProductionOrders = new List<T_ProductionOrder>();
            var perfectFittingProductionOrders = new List<T_ProductionOrder>();
            var pos = ProductionOrders.Include(b => b.ProductionOrderWorkSchedule)
                                    .Include(a => a.DemandProviderProductionOrders)
                                    .Where(a => a.ArticleId == demand.ArticleId 
                                        && (GetLatestEndFromProductionOrder(a)== null
                                            || GetLatestEndFromProductionOrder(a) == 0
                                            || GetLatestEndFromProductionOrder(a)>=timer)).ToList();
            foreach (var po in pos)
            {
                var availableAmount = GetAvailableAmountFromProductionOrder(po);
                if (availableAmount == amount) perfectFittingProductionOrders.Add(po);
                else if (availableAmount > 0) possibleProductionOrders.Add(po);
            }
            if (!possibleProductionOrders.Any()) return null;
            if (perfectFittingProductionOrders.Any())
                return new List<T_ProductionOrder>() {GetEarliestProductionOrder(perfectFittingProductionOrders)};
            var list = new List<T_ProductionOrder>();
            while (amount > 0 && possibleProductionOrders.Any())
            {
                list.Add(GetEarliestProductionOrder(possibleProductionOrders));
                possibleProductionOrders.Remove(list.Last());
                amount -= GetAvailableAmountFromProductionOrder(list.Last());
            }
            return list;
        }

        private int? GetLatestEndFromProductionOrder(T_ProductionOrder po)
        {
            if (po.ProductionOrderWorkSchedule == null || !po.ProductionOrderWorkSchedule.Any()) return null;
            var maxEnd = po.ProductionOrderWorkSchedule.Max(a => a.End);
            return maxEnd;
        }
        

        public int GetDueTimeByOrder(IDemandToProvider demand)
        {
            
            if (demand.GetType() == typeof(DemandCustomerOrderPart))
            {
                
                demand = DemandToProviders.OfType<DemandCustomerOrderPart>()
                                .Include(a => a.OrderPart)
                                .ThenInclude(b => b.Order)
                                .ToList().Single(a => a.Id == demand.Id);
                return ((DemandCustomerOrderPart) demand).OrderPart.Order.DueTime;
            }
            if (demand.GetType() == typeof(DemandStock)) return int.MaxValue;
            if (demand.GetType() != typeof(DemandProductionOrderBom)) return int.MaxValue;
            {
                demand =
                    DemandToProviders.AsNoTracking().OfType<DemandProductionOrderBom>()
                        .Include(a => a.ProductionOrderBom)
                        .ThenInclude(b => b.ProductionOrderParent)
                        .Single(c => c.Id == demand.Id);
                return ((DemandProductionOrderBom) demand).ProductionOrderBom.ProductionOrderParent.Duetime;
            }
        }

        /// <summary>
        /// copies am Article and his Childs to ProductionOrder
        /// Creates Demand Provider for Production oder and DemandRequests for childs
        /// </summary>
        /// <returns></returns>
        public T_ProductionOrder CopyArticleToProductionOrder(int articleId, decimal quantity, int demandRequesterId)
        {
            var article = Articles.Include(a => a.ArticleBoms).ThenInclude(c => c.ArticleChild).Single(a => a.Id == articleId);

            var mainProductionOrder = new T_ProductionOrder
            {
                ArticleId = article.Id,
                Name = "Prod. Auftrag: " + article.Name,
                Quantity = quantity,
            };
            ProductionOrders.Add(mainProductionOrder);

            CreateProductionOrderWorkSchedules(mainProductionOrder);

           
            var demandProvider = new ProviderProductionOrder()
            {
                ProductionOrderId = mainProductionOrder.Id,
                Quantity = quantity,
                ArticleId = article.Id,
                DemandRequesterId = demandRequesterId,
            };
            DemandToProviders.Add(demandProvider);

            SaveChanges();

            return mainProductionOrder;
        }

        /*public ProductionOrder CreateBomForProductionOrder(decimal quantity, ProductionOrder mainProductionOrder)
        {
            var article = Articles.Include(a => a.ArticleBoms).ThenInclude(c => c.ArticleChild).Single(a => a.Id == mainProductionOrder.ArticleId);
            foreach (var item in article.ArticleBoms)
            {
                var thisQuantity = quantity * item.Quantity;

                var prodOrder = new ProductionOrder
                {
                    ArticleId = item.ArticleChildId,
                    Name = "Prod. Auftrag: " + article.Name,
                    Quantity = thisQuantity,
                };
                ProductionOrders.Add(prodOrder);

                var prodOrderBom = new ProductionOrderBom
                {
                    Quantity = thisQuantity,
                    ProductionOrderParentId = mainProductionOrder.Id,

                };
                mainProductionOrder.ProductionOrderBoms.Add(prodOrderBom);

                CreateDemandProductionOrderBom(item.ArticleChildId, thisQuantity);
            }
            SaveChanges();
            return mainProductionOrder;
        }*/


        public void CreateProductionOrderWorkSchedules(T_ProductionOrder productionOrder)
        {
            var abstractWorkSchedules = Operations.Where(a => a.ArticleId == productionOrder.ArticleId).ToList();
            foreach (var abstractWorkSchedule in abstractWorkSchedules)
            {
                //add specific workSchedule
                var workSchedule = new T_ProductionOrderOperation();
                abstractWorkSchedule.CopyPropertiesTo<IWorkSchedule>(workSchedule);
                workSchedule.ProductionOrderId = productionOrder.Id;
                workSchedule.MachineId = null;
                workSchedule.ProducingState = ProducingState.Created;
                workSchedule.Duration *= (int)productionOrder.Quantity;
                ProductionOrderOperations.Add(workSchedule);
                SaveChanges();
            }
        }

        public IDemandToProvider CreateDemandOrderPart(T_CustomerOrderPart orderPart)
        {
            var demand = new DemandCustomerOrderPart()
            {
                OrderPartId = orderPart.Id,
                Quantity = orderPart.Quantity,
                Article = orderPart.Article,
                ArticleId = orderPart.ArticleId,
                OrderPart = orderPart,
                DemandProvider = new List<T_Demand>(),
                State = State.Created
            };
            DemandToProviders.Add(demand);
            SaveChanges();
            return demand;
        }

        public List<IDemandToProvider> UpdateStateDemandProviderPurchaseParts()
        {
            var changedDemands = new List<IDemandToProvider>();
            var provider = DemandToProviders.OfType<ProviderPurchasePart>().Include(a => a.PurchasePart).ThenInclude(b => b.Purchase).Where(a => a.State != State.Finished).ToList();
            foreach (var singleProvider in provider)
            {
                if (singleProvider.PurchasePart.State != State.Finished) continue;
                singleProvider.State = State.Finished;
                changedDemands.Add(singleProvider);
                Update(singleProvider);
                SaveChanges();
            }
            return changedDemands;
        }

        public bool TryUpdateStockProvider(T_Demand requester)
        {
            var unfinishedProvider = (from dp in requester.DemandProvider
                                      where dp.State != State.Finished
                                      select dp).ToList();
            if (unfinishedProvider.Any(d => d.GetType() != typeof(ProviderStock)))
                return false;
            foreach (var provider in unfinishedProvider)
            {
                provider.State = State.Finished;
            }
            return true;
        }

        public List<IDemandToProvider> UpdateStateDemandRequester(List<IDemandToProvider> provider)
        {
            var changedRequester = new List<IDemandToProvider>();
            foreach (var singleProvider in provider)
            {
                var prov = DemandToProviders.Include(a => a.DemandRequester).ThenInclude(b => b.DemandProvider).Single(a => a.Id == singleProvider.Id);
                if (prov.DemandRequester.DemandProvider.Any(a => a.State != State.Finished))
                {
                    if (!TryUpdateStockProvider(prov.DemandRequester))
                        continue;
                }
                //check for PO then set ready if it has started and taken out the articles from the stock
                if (prov.DemandRequester.GetType() == typeof(DemandProductionOrderBom))
                {
                    var dpob = DemandToProviders.OfType<DemandProductionOrderBom>().Single(a => a.Id == prov.DemandRequester.Id);
                    if (dpob.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule.All(a => a.ProducingState == ProducingState.Created))
                        continue;
                }
                    
                prov.DemandRequester.State = State.Finished;
                Update(prov.DemandRequester);
                SaveChanges();
                changedRequester.Add(prov.DemandRequester);
            }
            var test = DemandToProviders.OfType<DemandProductionOrderBom>().Where(a => a.State != State.Finished);
            var test2 = test.Where(a => a.DemandProvider.All(b => b.State == State.Finished));
            Debug.WriteLine(test2.Count() +" items waiting to be concluded");
            Debug.WriteLine(test2.Count(a => a.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule.First().Name == "Wedding"));
            
            var dpobs = DemandToProviders.OfType<DemandProductionOrderBom>().Where(a => a.State!=State.Finished 
                                                                                    && a.ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule.Any(c => c.ProducingState!=ProducingState.Created));
            foreach (var dpob in dpobs)
            {
                dpob.State = State.Finished;
                changedRequester.Add(dpob);
            }
            return changedRequester;
        }

        public T_CustomerOrder CreateNewOrder(int articleId, int amount, int creationTime, int dueTime)
        {
            var olist = new List<T_CustomerOrderPart>();
            olist.Add(new T_CustomerOrderPart
            {
                ArticleId = articleId,
                IsPlanned = false,
                Quantity = amount,
            });

            var order = new T_CustomerOrder()
            {
                BusinessPartnerId = BusinessPartners.First(x => x.Debitor).Id,
                DueTime = dueTime,
                CreationTime = creationTime,
                Name = Articles.Single(x => x.Id == articleId).Name,
                OrderParts = olist
            };
            return order;
        }



        public int GetEarliestStart(ResultContext kpiContext, SimulationWorkschedule simulationWorkschedule, SimulationType simulationType, int simulationId,  List<SimulationWorkschedule> schedules = null)
        {
            if (simulationType == SimulationType.Central)
            {
                var orderId = simulationWorkschedule.OrderId.Replace("[", "").Replace("]", "");
                var start = kpiContext.SimulationOperations
                    .Where(x => x.SimulationConfigurationId == simulationId && x.SimulationType == simulationType)
                    .Where(a =>
                    a.OrderId.Equals("[" + orderId.ToString() + ",")
                    || a.OrderId.Equals("," + orderId.ToString() + "]")
                    || a.OrderId.Equals("[" + orderId.ToString() + "]")
                    || a.OrderId.Equals("," + orderId.ToString() + ",")).Min(b => b.Start);
                return start;
            }

            var children = new List<SimulationWorkschedule>();
            children = schedules.Where(x => x.SimulationConfigurationId == simulationId && x.SimulationType == simulationType)
                                .Where(a => a.ParentId.Equals(simulationWorkschedule.ProductionOrderId.ToString())).ToList();
            
            if (!children.Any()) return simulationWorkschedule.Start;
            var startTimes = children.Select(child => GetEarliestStart(kpiContext, child, simulationType, simulationId, schedules)).ToList();
            return startTimes.Min();
        }
    }
}
