using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Master40.DB.Interfaces;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Context
{
    public class ProductionDomainContext : MasterDBContext
    {
        public ProductionDomainContext(DbContextOptions<MasterDBContext> options) : base(options) { }


        public Order OrderById(int id)
        {
            return Orders.FirstOrDefault(x => x.Id == id);
        }
        /// <summary>
        /// Receives the prior items to a given ProductionOrderWorkSchedule
        /// </summary>
        /// <param name="productionOrderWorkSchedule"></param>
        /// <returns>List<ProductionOrderWorkSchedule></returns>
        public Task<List<ProductionOrderWorkSchedule>> GetFollowerProductionOrderWorkSchedules(ProductionOrderWorkSchedule productionOrderWorkSchedule)
        {
            var rs = Task.Run(() =>
            {
                var priorItems = new List<ProductionOrderWorkSchedule>();
                // If == min Hierarchy --> get Pevious Article -> Highest Hierarchy Workschedule Item
                var maxHierarchy = ProductionOrderWorkSchedules.Where(x => x.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId)
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
                        ProductionOrderWorkSchedules.Where(
                                x => x.ProductionOrderId == productionOrderWorkSchedule.ProductionOrderId
                                     && x.HierarchyNumber > productionOrderWorkSchedule.HierarchyNumber)
                            .OrderBy(x => x.HierarchyNumber).FirstOrDefault();
                    priorItems.Add(previousPows);
                }
                return priorItems;
            });

            return rs;
        }

        public Task<List<SimulationWorkschedule>> GetFollowerProductionOrderWorkSchedules(SimulationWorkschedule simulationWorkSchedule)
        {
            var rs = Task.Run(() =>
            {
                var priorItems = new List<SimulationWorkschedule>();
                // If == min Hierarchy --> get Pevious Article -> Highest Hierarchy Workschedule Item
                var maxHierarchy = SimulationWorkschedules.Where(x => x.ProductionOrderId == simulationWorkSchedule.ProductionOrderId)
                    .Max(x => x.HierarchyNumber);

                if (maxHierarchy == simulationWorkSchedule.HierarchyNumber)
                {
                    // get Previous Article
                    priorItems.AddRange(SimulationWorkschedules
                        .Where(x => x.ProductionOrderId == simulationWorkSchedule.ParentId
                                && x.HierarchyNumber == SimulationWorkschedules
                                    .Where(w => w.ProductionOrderId == simulationWorkSchedule.ParentId)
                                    .Min(m => m.HierarchyNumber)));
                }
                else
                {
                    // get Previous Workschedule
                    var previousPows =
                        SimulationWorkschedules.Where(
                                x => x.ProductionOrderId == simulationWorkSchedule.ProductionOrderId
                                     && x.HierarchyNumber > simulationWorkSchedule.HierarchyNumber)
                            .OrderBy(x => x.HierarchyNumber).FirstOrDefault();
                    priorItems.Add(previousPows);
                }
                return priorItems;
            });
            return rs;
        }

        public List<ProductionOrderWorkSchedule> GetParents(ProductionOrderWorkSchedule schedule)
        {
            if (schedule == null) return null;
            var parents = new List<ProductionOrderWorkSchedule>();
            var parent = GetHierarchyParent(schedule);
            if (parent != null) parents.Add(parent);
            else parents.AddRange(GetBomParents(schedule));
            return parents;
        }

        public ProductionOrderWorkSchedule GetHierarchyParent(ProductionOrderWorkSchedule pows)
        {

            ProductionOrderWorkSchedule hierarchyParent = null;
            int hierarchyParentNumber = 100000;
            //find next higher element
            foreach (var mainSchedule in pows.ProductionOrder.ProductionOrderWorkSchedule)
            {
                if (mainSchedule.ProductionOrderId != pows.ProductionOrderId) continue;
                if (mainSchedule.HierarchyNumber <= pows.HierarchyNumber ||
                    mainSchedule.HierarchyNumber >= hierarchyParentNumber) continue;
                hierarchyParent = mainSchedule;
                hierarchyParentNumber = mainSchedule.HierarchyNumber;
            }
            return hierarchyParent;
        }

        public List<ProductionOrderWorkSchedule> GetBomParents(ProductionOrderWorkSchedule plannedSchedule)
        {
            var provider = plannedSchedule.ProductionOrder.DemandProviderProductionOrders;
            return (from demandProviderProductionOrder in provider
                    select demandProviderProductionOrder.DemandRequester into requester
                    where requester.GetType() == typeof(DemandProductionOrderBom)
                    select ((DemandProductionOrderBom)requester).ProductionOrderBom.ProductionOrderParent.ProductionOrderWorkSchedule into schedules
                    select schedules.Single(a => a.HierarchyNumber == schedules.Min(b => b.HierarchyNumber))).ToList();
        }



        public async Task<Article> GetArticleBomRecursive(Article article, int articleId)
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
        public Task<List<ProductionOrderWorkSchedule>> GetProductionOrderWorkScheduleByOrderId(int orderId)
        {
            return Task.Run(() =>
            {
                // get the corresponding Order Parts to Order
                var demands = Demands.OfType<DemandOrderPart>()
                    .Include(x => x.OrderPart)
                    .Where(o => o.OrderPart.OrderId == orderId)
                    .ToList();

                // ReSharper Linq
                var demandboms = demands.SelectMany(demand => Demands.OfType<DemandProductionOrderBom>()
                    .Where(a => a.DemandRequesterId == demand.Id)).ToList();

                // get Demand Providers for this Order
                var demandProviders = new List<DemandProviderProductionOrder>();
                foreach (var order in (Demands.OfType<DemandProviderProductionOrder>()
                    .Join(demands, c => c.DemandRequesterId, d => ((IDemandToProvider)d).Id, (c, d) => c)))
                {
                    demandProviders.Add(order);
                }

                var demandBomProviders = (Demands.OfType<DemandProviderProductionOrder>()
                    .Join(demandboms, c => c.DemandRequesterId, d => d.Id, (c, d) => c)).ToList();

                // get ProductionOrderWorkSchedule for 
                var pows = ProductionOrderWorkSchedules.Include(x => x.ProductionOrder).Include(x => x.Machine).Include(x => x.MachineGroup).AsNoTracking();
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
        public List<int> GetOrderIdsFromProductionOrder(ProductionOrder po)
        {
            po = ProductionOrders.Include(a => a.DemandProviderProductionOrders).ThenInclude(b => b.DemandRequester).Single(a => a.Id == po.Id);
            var ids = new List<int>();
            var requester = from provider in po.DemandProviderProductionOrders
                select provider.DemandRequester;
            foreach (var singleRequester in requester)
            {
                if (singleRequester.GetType() == typeof(DemandProductionOrderBom))
                {
                    
                    ids.AddRange(GetOrderIdsFromProductionOrder(
                        ((DemandProductionOrderBom) singleRequester).ProductionOrderBom.ProductionOrderParent));
                }
                else if (singleRequester.GetType() == typeof(DemandOrderPart))
                {
                    var dop = Demands.OfType<DemandOrderPart>().Include(a => a.OrderPart).Single(a => a.Id == singleRequester.Id);
                    return new List<int>()
                        {
                            dop.OrderPart.OrderId
                        };
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
        public List<ProductionOrderWorkSchedule> GetWorkSchedulesFromDemand(IDemandToProvider demandRequester, ref List<ProductionOrderWorkSchedule> productionOrderWorkSchedule)
        {
            foreach (var item in demandRequester.DemandProvider.OfType<DemandProviderProductionOrder>())
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

        public List<ProductionOrder> CreateChildProductionOrders(IDemandToProvider demand, decimal amount, int simulationId)
        {
            ProductionOrderBom bom = null;
            if (demand.GetType() == typeof(DemandProductionOrderBom))
            {
                bom = ProductionOrderBoms.FirstOrDefault(a => a.Id == ((DemandProductionOrderBom) demand).ProductionOrderBomId);
            }
            
            var lotsize = SimulationConfigurations.Single(a => a.Id == simulationId).Lotsize;
            var productionOrders = new List<ProductionOrder>();
            decimal bomQuantity;
            if (bom != null)
                bomQuantity = ArticleBoms.AsNoTracking().Single(a => a.ArticleChildId == demand.ArticleId &&
                    a.ArticleParentId == bom.ProductionOrderParent.ArticleId).Quantity * lotsize;
            else 
                bomQuantity = ArticleBoms.AsNoTracking().Single(a =>a.ArticleChildId == demand.ArticleId && 
                    a.ArticleParentId == null).Quantity * lotsize;
            //Todo: check ||
            while (amount > 0 || bomQuantity > 0)
            {
                var productionOrder = CreateProductionOrder(demand,GetDueTimeByOrder(demand),simulationId);
                if (amount > 0)
                {
                    var demandProviderProductionOrder = CreateDemandProviderProductionOrder(demand, productionOrder,
                        amount > lotsize ? lotsize : amount);

                    //if the article has a parent create a relationship
                    if (bom != null)
                    {
                        demandProviderProductionOrder.DemandRequesterId = demand.Id;
                        Demands.Update(demandProviderProductionOrder);
                    }
                }
                SaveChanges();
                amount -= lotsize;
                bomQuantity -= lotsize;
                productionOrders.Add(productionOrder);
            }
            
            return productionOrders;
        }

        public DemandStock CreateStockDemand(IDemandToProvider demand, Stock stock, decimal amount)
        {
            var demandStock = new DemandStock()
            {
                Quantity = amount,
                Article = demand.Article,
                ArticleId = demand.ArticleId,
                State = State.Created,
                DemandProvider = new List<DemandToProvider>(),
                StockId = stock.Id,
            };
            Demands.Add(demandStock);
            SaveChanges();
            return demandStock;
        }

        public ProductionOrder CreateProductionOrder(IDemandToProvider demand, int duetime, int simulationId)
        {
            var productionOrder = new ProductionOrder()
            {
                ArticleId = demand.Article.Id,
                Quantity = SimulationConfigurations.Single(a => a.Id == simulationId).Lotsize,
                Duetime = duetime
            };
            
            ProductionOrders.Add(productionOrder);
            SaveChanges();
            return productionOrder;
        }

        public decimal GetPlannedStock(Stock stock, IDemandToProvider demand)
        {
            var amountReserved = GetReserved(demand.ArticleId);
            var amountBought = 0;
            var articlesBought = PurchaseParts.Where(a => a.ArticleId == demand.ArticleId && a.State != State.Finished);
            foreach (var articleBought in articlesBought)
            {
                amountBought += articleBought.Quantity;
            }
            //just produced articles have a reason and parents they got produced for so they cannot be reserved by another requester
            var amountJustProduced = Demands.OfType<DemandProductionOrderBom>()
                .Where(a => a.State != State.Finished && a.ArticleId == demand.ArticleId && a.DemandProvider.All(b => b.State == State.Finished)).Sum(a => a.Quantity);
            //plannedStock is the amount of this article in stock after taking out the amount needed
            var plannedStock = stock.Current + amountBought - demand.Quantity - amountReserved - amountJustProduced;

            return plannedStock;
        }

        public DemandProviderProductionOrder CreateDemandProviderProductionOrder(IDemandToProvider demand, ProductionOrder productionOrder, decimal amount)
        {
            var demandProviderProductionOrder = new DemandProviderProductionOrder()
            {
                Quantity = amount,
                Article = demand.Article,
                ArticleId = demand.ArticleId,
                ProductionOrderId = productionOrder.Id,
                DemandRequesterId = demand.Id
            };
            Demands.Add(demandProviderProductionOrder);
            if (productionOrder.DemandProviderProductionOrders == null) productionOrder.DemandProviderProductionOrders = new List<DemandProviderProductionOrder>();
            productionOrder.DemandProviderProductionOrders.Add(demandProviderProductionOrder);
            SaveChanges();

            return demandProviderProductionOrder;
        }

        //Todo: check logic
        public void CreatePurchaseDemand(IDemandToProvider demand, decimal amount)
        {
            if (NeedToRefill(demand, amount))
            {
                var providerPurchasePart = new DemandProviderPurchasePart()
                {
                    Quantity = amount,
                    ArticleId = demand.ArticleId,
                    DemandRequesterId = demand.Id,
                    State = State.Created,
                };
                Demands.Add(providerPurchasePart);

                CreatePurchase(demand, amount, providerPurchasePart);
                Demands.Update(providerPurchasePart);
            }
            else
            {
                var providerStock = new DemandProviderStock()
                {
                    Quantity = amount,
                    ArticleId = demand.ArticleId,
                    DemandRequesterId = demand.Id,
                    State = State.Created,
                    StockId = Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Id
                };
                Demands.Add(providerStock);
            }
            SaveChanges();
        }

        public void CreatePurchase(IDemandToProvider demand, decimal amount, DemandProviderPurchasePart demandProviderPurchasePart)
        {
            var articleToPurchase = ArticleToBusinessPartners.Single(a => a.ArticleId == demand.ArticleId);
            var purchase = new Purchase()
            {
                BusinessPartnerId = articleToPurchase.BusinessPartnerId,
                DueTime = articleToPurchase.DueTime
            };
            //amount to be purchased has to be raised to fit the packsize
            amount = Math.Ceiling(amount / articleToPurchase.PackSize) * articleToPurchase.PackSize;
            var purchasePart = new PurchasePart()
            {
                ArticleId = demand.ArticleId,
                Quantity = (int)amount,
                DemandProviderPurchaseParts = new List<DemandProviderPurchasePart>() { demandProviderPurchasePart },
                PurchaseId = purchase.Id
            };
            purchase.PurchaseParts = new List<PurchasePart>()
            {
                purchasePart
            };

            Purchases.Add(purchase);
            PurchaseParts.Add(purchasePart);
            SaveChanges();
        }

        public bool NeedToRefill(IDemandToProvider demand, decimal amount)
        {
            var purchasedAmount = GetAmountBought(demand.ArticleId);
            var neededAmount = GetReserved(demand.ArticleId);
            var stockMin = Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId).Min;
            return (purchasedAmount - neededAmount - amount < stockMin);
        }

        public DemandProviderStock CreateDemandProviderStock(IDemandToProvider demand, decimal amount)
        {
            var dps = new DemandProviderStock()
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
        public DemandProviderProductionOrder CreateProviderProductionOrder(IDemandToProvider demand, ProductionOrder productionOrder, decimal amount)
        {
            var dppo = new DemandProviderProductionOrder()
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
        public DemandProviderPurchasePart CreateDemandProviderPurchasePart(IDemandToProvider demand, PurchasePart purchase, decimal amount)
        {
            var dppp = new DemandProviderPurchasePart()
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


        public void AssignDemandProviderToProductionOrderBom(DemandProductionOrderBom demand, ProductionOrderBom pob)
        {
            if (pob.DemandProductionOrderBoms == null)
                pob.DemandProductionOrderBoms = new List<DemandProductionOrderBom>();
            pob.DemandProductionOrderBoms.Add(demand);
            demand.ProductionOrderBomId = pob.Id;
            Update(demand);
            Update(pob);
            SaveChanges();
        }

        public void AssignProviderToDemand(IDemandToProvider demand, DemandToProvider provider)
        {
            if (demand.DemandProvider == null) demand.DemandProvider = new List<DemandToProvider>();
            demand.DemandProvider.Add(provider);
            Update(demand);
            SaveChanges();
        }

        public ProductionOrderBom TryCreateProductionOrderBoms(IDemandToProvider demand, ProductionOrder parentProductionOrder, int simulationId)
        {
            if (parentProductionOrder == null) return null;
            var lotsize = SimulationConfigurations.Single(a => a.Id == simulationId).Lotsize;
            var quantity = demand.Quantity > lotsize ? lotsize : demand.Quantity;
            var pob = new ProductionOrderBom()
            {
                Quantity = quantity,
                ProductionOrderParentId = parentProductionOrder.Id
            };
            Add(pob);
            SaveChanges();
            if (demand.GetType() == typeof(DemandProductionOrderBom))
                AssignDemandProviderToProductionOrderBom((DemandProductionOrderBom)demand, pob);
            return pob;
        }
        
        public void AssignPurchaseToDemandProvider(PurchasePart purchasePart, DemandProviderPurchasePart provider, int quantity)
        {
            provider.PurchasePartId = purchasePart.Id;
            provider.Quantity = quantity;
            Update(provider);
            SaveChanges();
        }

       
        public PurchasePart CreatePurchase(IDemandToProvider demand, decimal amount)
        {
            var articleToPurchase = ArticleToBusinessPartners.Single(a => a.ArticleId == demand.ArticleId);
            var purchase = new Purchase()
            {
                BusinessPartnerId = articleToPurchase.BusinessPartnerId,
                DueTime = articleToPurchase.DueTime
            };
            //amount to be purchased has to be raised to fit the packsize
            amount = Math.Ceiling(amount / articleToPurchase.PackSize) * articleToPurchase.PackSize;
            var purchasePart = new PurchasePart()
            {
                ArticleId = demand.ArticleId,
                Quantity = (int)amount,
                DemandProviderPurchaseParts = new List<DemandProviderPurchasePart>(),
                PurchaseId = purchase.Id
            };
            purchase.PurchaseParts = new List<PurchasePart>()
            {
                purchasePart
            };
            Purchases.Add(purchase);
            PurchaseParts.Add(purchasePart);
            SaveChanges();
            return purchasePart;
        }

        public int GetAvailableAmountFromProductionOrder(ProductionOrder productionOrder)
        {
            if (productionOrder.DemandProviderProductionOrders == null) return (int) productionOrder.Quantity;
            return (int)productionOrder.Quantity - productionOrder.DemandProviderProductionOrders.Sum(provider => (int)provider.Quantity);
        }

        public ProductionOrder GetEarliestProductionOrder(List<ProductionOrder> productionOrders)
        {
            ProductionOrder earliestProductionOrder = null;
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

        public DemandProviderStock TryCreateStockReservation(IDemandToProvider demand)
        {
            var stock = Stocks.Single(a => a.ArticleForeignKey == demand.ArticleId);
            var stockReservations = GetReserved(demand.ArticleId);
            var bought = GetAmountBought(demand.ArticleId);
            //get the current amount of free available articles
            var current = stock.Current + bought - stockReservations;
            decimal quantity;
            //either reserve all that are in stock or the amount needed
            quantity = demand.Quantity > current ? current : demand.Quantity;
            return quantity == 0 ? null : CreateDemandProviderStock(demand, quantity);
        }

        public decimal GetAmountBought(int articleId)
        {
            var purchaseParts = PurchaseParts.Where(a => a.ArticleId == articleId);
            var purchasedAmount = 0;
            foreach (var purchasePart in purchaseParts)
                purchasedAmount += purchasePart.Quantity;
            return purchasedAmount;
        }
        public decimal GetReserved(int articleId)
        {
            decimal amountReserved = 0;
            IQueryable<IDemandToProvider> reservations = Demands.OfType<DemandProviderStock>().Where(a => a.State != State.Finished && a.ArticleId == articleId);
            foreach (var reservation in reservations)
                amountReserved += reservation.Quantity;
            //Todo check logic
            reservations = Demands.OfType<DemandProviderPurchasePart>().Where(a => a.ArticleId == articleId && a.State != State.Finished);
            foreach (var reservation in reservations)
                amountReserved += reservation.Quantity;
            return amountReserved;
        }

        public DemandProductionOrderBom CreateDemandProductionOrderBom(int articleId, decimal quantity)
        {
            var dpob = new DemandProductionOrderBom()
            {
                Quantity = quantity,
                ArticleId = articleId,
                State = State.Created,
            };
            DemandProductionOrderBoms.Add(dpob);
            SaveChanges();
            return dpob;
        }

        public void AssignProductionOrderToDemandProvider(ProductionOrder productionOrder, DemandProviderProductionOrder provider)
        {
            if (productionOrder.DemandProviderProductionOrders == null) productionOrder.DemandProviderProductionOrders = new List<DemandProviderProductionOrder>();
            if (!productionOrder.DemandProviderProductionOrders.Contains(provider))
                productionOrder.DemandProviderProductionOrders.Add(provider);
            Update(provider);
            SaveChanges();
        }

        public bool HasChildren(IDemandToProvider demand)
        {
            return ArticleBoms.Any(a => a.ArticleParentId == demand.ArticleId);
        }
        
        public List<ProductionOrder> CheckForProductionOrders(IDemandToProvider demand, decimal amount, int timer)
        {
            var possibleProductionOrders = new List<ProductionOrder>();
            var perfectFittingProductionOrders = new List<ProductionOrder>();
            var pos = ProductionOrders.Include(b => b.ProductionOrderWorkSchedule)
                                    .Include(a => a.DemandProviderProductionOrders)
                                    .Where(a => a.ArticleId == demand.ArticleId 
                                        && (GetLatestEndFromProductionOrder(a)== null 
                                            || GetLatestEndFromProductionOrder(a)>=timer)).ToList();
            foreach (var po in pos)
            {
                var availableAmount = GetAvailableAmountFromProductionOrder(po);
                if (availableAmount == amount) perfectFittingProductionOrders.Add(po);
                else if (availableAmount > 0) possibleProductionOrders.Add(po);
            }
            if (!possibleProductionOrders.Any()) return null;
            if (perfectFittingProductionOrders.Any())
                return new List<ProductionOrder>() {GetEarliestProductionOrder(perfectFittingProductionOrders)};
            var list = new List<ProductionOrder>();
            while (amount > 0 && possibleProductionOrders.Any())
            {
                list.Add(GetEarliestProductionOrder(possibleProductionOrders));
                possibleProductionOrders.Remove(list.Last());
                amount -= GetAvailableAmountFromProductionOrder(list.Last());
            }
            return list;
        }

        private int? GetLatestEndFromProductionOrder(ProductionOrder po)
        {
            if (po.ProductionOrderWorkSchedule == null || !po.ProductionOrderWorkSchedule.Any()) return null;
            var maxEnd = po.ProductionOrderWorkSchedule.Max(a => a.End);
            return maxEnd;
        }
        

        public int GetDueTimeByOrder(IDemandToProvider demand)
        {
            
            if (demand.GetType() == typeof(DemandOrderPart))
            {
                
                demand = Demands.OfType<DemandOrderPart>().Include(a => a.OrderPart).ThenInclude(b => b.Order).ToList().Single(a => a.Id == demand.Id);
                return ((DemandOrderPart) demand).OrderPart.Order.DueTime;
            }
            if (demand.GetType() == typeof(DemandStock)) return 999999;
            if (demand.GetType() != typeof(DemandProductionOrderBom)) return 99999999;
            {
                demand =
                    Demands.AsNoTracking().OfType<DemandProductionOrderBom>()
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
        public ProductionOrder CopyArticleToProductionOrder(int articleId, decimal quantity, int demandRequesterId)
        {
            var article = Articles.Include(a => a.ArticleBoms).ThenInclude(c => c.ArticleChild).Single(a => a.Id == articleId);

            var mainProductionOrder = new ProductionOrder
            {
                ArticleId = article.Id,
                Name = "Prod. Auftrag: " + article.Name,
                Quantity = quantity,
            };
            ProductionOrders.Add(mainProductionOrder);

            CreateProductionOrderWorkSchedules(mainProductionOrder);

           
            var demandProvider = new DemandProviderProductionOrder()
            {
                ProductionOrderId = mainProductionOrder.Id,
                Quantity = quantity,
                ArticleId = article.Id,
                DemandRequesterId = demandRequesterId,
            };
            Demands.Add(demandProvider);

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


        public void CreateProductionOrderWorkSchedules(ProductionOrder productionOrder)
        {
            var abstractWorkSchedules = WorkSchedules.Where(a => a.ArticleId == productionOrder.ArticleId).ToList();
            foreach (var abstractWorkSchedule in abstractWorkSchedules)
            {
                //add specific workSchedule
                var workSchedule = new ProductionOrderWorkSchedule();
                abstractWorkSchedule.CopyPropertiesTo<IWorkSchedule>(workSchedule);
                workSchedule.ProductionOrderId = productionOrder.Id;
                workSchedule.MachineId = null;
                workSchedule.ProducingState = ProducingState.Created;
                workSchedule.Duration *= (int)productionOrder.Quantity;
                ProductionOrderWorkSchedules.Add(workSchedule);
                SaveChanges();
            }
        }

        public IDemandToProvider CreateDemandOrderPart(OrderPart orderPart)
        {
            var demand = new DemandOrderPart()
            {
                OrderPartId = orderPart.Id,
                Quantity = orderPart.Quantity,
                Article = orderPart.Article,
                ArticleId = orderPart.ArticleId,
                OrderPart = orderPart,
                DemandProvider = new List<DemandToProvider>(),
                State = State.Created
            };
            Demands.Add(demand);
            SaveChanges();
            return demand;
        }

        public List<IDemandToProvider> UpdateStateDemandProviderPurchaseParts()
        {
            var changedDemands = new List<IDemandToProvider>();
            var provider = Demands.OfType<DemandProviderPurchasePart>().Include(a => a.PurchasePart).ThenInclude(b => b.Purchase).Where(a => a.State != State.Finished).ToList();
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

        public bool TryUpdateStockProvider(DemandToProvider requester)
        {
            var unfinishedProvider = (from dp in requester.DemandProvider
                                      where dp.State != State.Finished
                                      select dp).ToList();
            if (unfinishedProvider.Any(d => d.GetType() != typeof(DemandProviderStock)))
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
                var prov = Demands.Include(a => a.DemandRequester).ThenInclude(b => b.DemandProvider).Single(a => a.Id == singleProvider.Id);
                if (prov.DemandRequester.DemandProvider.Any(a => a.State != State.Finished))
                {
                    if (!TryUpdateStockProvider(prov.DemandRequester))
                        continue;
                }
                //check for PO then set ready if it has started and taken out the articles from the stock
                if (prov.DemandRequester.GetType() == typeof(DemandProductionOrderBom))
                {
                    var dpob = Demands.OfType<DemandProductionOrderBom>().Single(a => a.Id == prov.DemandRequester.Id);
                    if (!dpob.ProductionOrderBom.ProductionOrderParent
                        .ProductionOrderWorkSchedule.Any(a => a.ProducingState > ProducingState.Created))
                        continue;
                }
                    
                prov.DemandRequester.State = State.Finished;
                Update(prov.DemandRequester);
                SaveChanges();
                changedRequester.Add(prov.DemandRequester);
            }
            return changedRequester;
        }


        public int GetSimulationNumber(int simulationConfigurationId, SimulationType simType)
        {
            var anySim = SimulationWorkschedules
                .Where(x => x.SimulationType == simType && x.SimulationConfigurationId == simulationConfigurationId);
            return anySim.Any() ? anySim.Max(x => x.SimulationNumber) : 1;
        }

        public void CreateNewOrder(int articleId, int amount,int creationTime, int dueTime)
        {
            var order = new Order()
            {
                BusinessPartnerId = BusinessPartners.First(x => x.Debitor).Id,
                DueTime = dueTime,
                CreationTime = creationTime,
                Name = Articles.Single(x => x.Id == articleId).Name

            };
            Orders.Add(order);
            SaveChanges();
            var orderpart = new OrderPart()
            {
                ArticleId = articleId,
                IsPlanned = false,
                Quantity = amount,
                OrderId = order.Id
            };
            // order.OrderParts.Add(orderpart);
            OrderParts.Add(orderpart);
            SaveChanges();
        }

        public int GetEarliestStart(ProductionDomainContext context, SimulationWorkschedule simulationWorkschedule, SimulationType simulationType)
        {
            var children = new List<SimulationWorkschedule>();
            if (simulationType == SimulationType.Central)
                children = context.SimulationWorkschedules.Where(a => a.ParentId.Equals("[" + simulationWorkschedule.Id.ToString() + "]")).ToList();
            else // decentral
                children = context.SimulationWorkschedules.Where(a => a.ParentId.Equals(simulationWorkschedule.ProductionOrderId.ToString())).ToList();

            if (!children.Any()) return simulationWorkschedule.Start;
            var startTimes = new List<int>();
            foreach (var child in children)
            {
                startTimes.Add(GetEarliestStart(context, child, simulationType));
            }
            return startTimes.Min();
        }
    }
}
