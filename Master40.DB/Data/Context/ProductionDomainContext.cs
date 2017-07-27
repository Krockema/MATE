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
        /// Recives the prior items to a given ProductionOrderWorkSchedule
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
                    var priorBom = ProductionOrderBoms
                                                .Include(x => x.ProductionOrderParent)
                                                    .ThenInclude(x => x.ProductionOrderWorkSchedule)
                                                .Where(x => x.ProductionOrderChildId == productionOrderWorkSchedule.ProductionOrderId).ToList();

                    // out of each Part get Highest Workschedule
                    priorItems.AddRange(priorBom.Select(item => item.ProductionOrderParent.ProductionOrderWorkSchedule
                                                .OrderBy(x => x.HierarchyNumber).FirstOrDefault())
                                                .Where(prior => prior != null));
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


        public async Task<ProductionOrder> GetProductionOrderBomRecursive(ProductionOrder prodOrder, int productionOrderId)
        {

            prodOrder.ProdProductionOrderBomChilds = ProductionOrderBoms
                                                            .Include(a => a.ProductionOrderChild)
                                                            .ThenInclude(w => w.ProductionOrderWorkSchedule)
                                                            .Where(a => a.ProductionOrderParentId == productionOrderId).ToList();

            foreach (var item in prodOrder.ProdProductionOrderBomChilds)
            {
                await GetProductionOrderBomRecursive(item.ProductionOrderParent, item.ProductionOrderChildId);
            }
            await Task.Yield();
            return prodOrder;

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
        /// returns the OrderId for the ProductionOrderWorkSchedule
        /// </summary>
        /// <param name="pow"></param>
        /// <returns></returns>
        public int GetOrderIdFromProductionOrderWorkSchedule(ProductionOrderWorkSchedule pow)
        {
            //call requester.requester to make sure that also the DemandProductionOrderBoms find the DemandOrderPart
            var requester = (DemandOrderPart)pow.ProductionOrder.DemandProviderProductionOrders.First().DemandRequester.DemandRequester;
            return OrderParts.Single(a => a.Id == requester.OrderPartId).OrderId;
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
        }

        public List<ProductionOrder> CreateChildProductionOrders(IDemandToProvider demand, ProductionOrder parentProductionOrder, decimal amount)
        {
            var lotsize = SimulationConfigurations.Last().Lotsize;
            var productionOrders = new List<ProductionOrder>();
            decimal bomQuantity;
            if (parentProductionOrder != null)
                bomQuantity = ArticleBoms.AsNoTracking().Single(a => a.ArticleChildId == demand.ArticleId &&
                    a.ArticleParentId == parentProductionOrder.ArticleId).Quantity * lotsize;
            else 
                bomQuantity = ArticleBoms.AsNoTracking().Single(a =>a.ArticleChildId == demand.ArticleId && 
                    a.ArticleParentId == null).Quantity * lotsize;
            while (amount > 0 || bomQuantity > 0)
            {
                var productionOrder = CreateProductionOrder(demand,parentProductionOrder?.Duetime ?? GetDueTime(demand));
                if (amount > 0)
                {
                    var demandProviderProductionOrder = CreateDemandProviderProductionOrder(demand, productionOrder,
                        amount > lotsize ? lotsize : amount);

                    //if the article has a parent create a relationship
                    if (parentProductionOrder != null)
                    {
                        demandProviderProductionOrder.DemandRequesterId = demand.Id;
                        Demands.Update(demandProviderProductionOrder);
                        TryCreateProductionOrderBoms(demand, productionOrder, parentProductionOrder);
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

        public ProductionOrder CreateProductionOrder(IDemandToProvider demand, int duetime)
        {
            var productionOrder = new ProductionOrder()
            {
                ArticleId = demand.Article.Id,
                Quantity = SimulationConfigurations.Last().Lotsize,
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
            var articlesBought = PurchaseParts.Where(a => a.ArticleId == demand.ArticleId);
            foreach (var articleBought in articlesBought)
            {
                amountBought += articleBought.Quantity;
            }
            //plannedStock is the amount of this article in stock after taking out the amount needed
            var plannedStock = stock.Current + amountBought - demand.Quantity - amountReserved;

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
        public DemandProviderPurchasePart CreateProviderPurchase(IDemandToProvider demand, PurchasePart purchase, decimal amount)
        {
            var dppp = new DemandProviderPurchasePart()
            {
                ArticleId = demand.ArticleId,
                Quantity = amount,
                PurchasePartId = purchase.Id,
                DemandRequesterId = demand.DemandRequesterId,
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

        public void TryCreateProductionOrderBoms(IDemandToProvider demand, ProductionOrder productionOrder, ProductionOrder parentProductionOrder)
        {
            if (parentProductionOrder == null) return;
            var lotsize = SimulationConfigurations.Last().Lotsize;
            var bom = ArticleBoms.Where(a => a.ArticleChildId == demand.ArticleId);
            if (!bom.Any()) return;
            var absoluteQuantity = bom.ToList().Find(a => a.ArticleParentId == parentProductionOrder.ArticleId).Quantity * parentProductionOrder.Quantity;
            var pob = new ProductionOrderBom()
            {
                //Todo: check logic
                ProductionOrderChildId = productionOrder.Id,
                Quantity = absoluteQuantity > lotsize ? lotsize : absoluteQuantity,
                ProductionOrderParentId = parentProductionOrder.Id
            };
            Add(pob);
            SaveChanges();
            if (demand.GetType() == typeof(DemandProductionOrderBom))
                AssignDemandProviderToProductionOrderBom((DemandProductionOrderBom)demand, pob);
        }


        // Was das ? 
        // ////////////////////////////////////////////////////////

        public void AssignProductionOrderWorkSchedulesToProductionOrder(ProductionOrder productionOrder)
        {
            foreach (var pows in ProductionOrderWorkSchedules.Where(a => a.ProductionOrderId == productionOrder.Id))
            {
                productionOrder.ProductionOrderWorkSchedule.Add(pows);
            }
            Update(productionOrder);
            SaveChanges();
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
            IQueryable<IDemandToProvider> reservations = Demands.OfType<DemandProviderStock>().Where(a => a.ArticleId == articleId);
            foreach (var reservation in reservations)
                amountReserved += reservation.Quantity;
            //Todo check logic
            reservations = Demands.OfType<DemandProviderPurchasePart>().Where(a => a.ArticleId == articleId);
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
            Add(dpob);
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

        public void TryAssignProductionOrderBomToDemandProvider(ProductionOrder productionOrder, IDemandToProvider requester)
        {
            if (requester.GetType() != typeof(DemandProductionOrderBom)) return;
            var productionOrderBoms = ProductionOrderBoms.Where(a => a.ProductionOrderChildId == productionOrder.Id).ToList();
            if (productionOrderBoms.Count != 1) return;
            ((DemandProductionOrderBom)requester).ProductionOrderBomId = productionOrderBoms.First().Id;
            Update(requester);
            SaveChanges();
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

        public int GetDueTime(IDemandToProvider demand)
        {
            var dueTime = 9999;
            if (demand.GetType() == typeof(DemandOrderPart))
            {
                dueTime = OrderParts
                            .Include(a => a.Order)
                            .Single(a => a.Id == ((DemandOrderPart)demand).OrderPartId)
                            .Order
                            .DueTime;
            }
            return dueTime;
        }

        /// <summary>
        ///  USE IS DEPRICATED Better user GetProductionOrderWorkSchedules(demand, )
        /// </summary>
        /// <param name="demand"></param>
        /// <returns></returns>
        //returns a list of all workSchedules for the given orderPart and planningType
        public List<ProductionOrderWorkSchedule> GetProductionOrderWorkSchedules(IDemandToProvider demand)
        {
            
            var pows = new List<ProductionOrderWorkSchedule>();
            var pos = from prov in demand.DemandProvider
                      where prov.GetType() == typeof(DemandProviderProductionOrder)
                      select ((DemandProviderProductionOrder)prov).ProductionOrder;
            foreach (var po in pos)
            {
                var productionOrder = ProductionOrders
                                        .Include(a => a.ProductionOrderBoms)
                                        .Include(a => a.ProductionOrderWorkSchedule)
                                        .Single(a => a.Id == po.Id);
                pows.AddRange(productionOrder.ProductionOrderWorkSchedule.Where(a => a.ProducingState == ProducingState.Created));
                var childrenBoms = productionOrder.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == productionOrder.Id);
                foreach (var childBom in childrenBoms)
                {
                    pows.AddRange(GetProductionOrderWorkSchedules(childBom.ProductionOrderChild));
                }
            }
            return pows;
        }
        private List<ProductionOrderWorkSchedule> GetProductionOrderWorkSchedules(ProductionOrder po)
        {
            var pows = new List<ProductionOrderWorkSchedule>();
            var productionOrder = ProductionOrders.Include(a => a.ProductionOrderBoms).Include(a => a.ProductionOrderWorkSchedule).Single(a => a.Id == po.Id);
            pows.AddRange(productionOrder.ProductionOrderWorkSchedule.Where(a => a.ProducingState == ProducingState.Created));
            var childrenBoms = productionOrder.ProductionOrderBoms.Where(a => a.ProductionOrderParentId == productionOrder.Id);
            foreach (var childBom in childrenBoms)
            {
                pows.AddRange(GetProductionOrderWorkSchedules(childBom.ProductionOrderChild));
            }
            return pows;
        }

        public int GetDueTimeByOrder(DemandToProvider demand)
        {
            if (demand.GetType() == typeof(DemandOrderPart)) return ((DemandOrderPart) demand).OrderPart.Order.DueTime;
            if (demand.GetType() == typeof(DemandStock)) return 999999;
            return 99999999;
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

        public ProductionOrder CreateBomForProductionOrder(decimal quantity, ProductionOrder mainProductionOrder)
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
                    ProductionOrderChildId = prodOrder.Id
                };
                mainProductionOrder.ProductionOrderBoms.Add(prodOrderBom);

                CreateDemandProductionOrderBom(item.ArticleChildId, thisQuantity);
            }
            SaveChanges();
            return mainProductionOrder;
        }


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

    }
}
