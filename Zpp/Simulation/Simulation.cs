using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.XUnitTest.DBContext;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Runtime.InteropServices;
using Dapper;
using Master40.DB.DataModel;
using Master40.SimulationCore.Helper;
using Zpp.Utils;


namespace Zpp.Simulation
{
    public class Simulation
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        
        private ProductionDomainContext _productionDomainContext;
        
        
        // MRP modules
        private readonly OrderManager.OrderManager  _orderManager;
        private readonly StockManager.StockManager  _stockManager = new StockManager.StockManager();
        private readonly ProductionManager.ProductionManager  _productionManager = new ProductionManager.ProductionManager();
        private readonly PurchaseManager.PurchaseManager  _purchaseManager = new PurchaseManager.PurchaseManager();

        private readonly bool _resetDb = false;
        
        public Simulation()
        {
            LOGGER.Info("Starting preparation for the ZPP simulation.");

            InitDb(_resetDb);
            InitModules();

            _orderManager = new OrderManager.OrderManager(_productionDomainContext);
        }

        private void InitDb(bool resetDb)
        {
            // If better sim performance is needed: InMemory
            // ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            // .UseInMemoryDatabase(databaseName: "InMemoryDB")
            // .Options); // InMemoryDB


            _productionDomainContext = Dbms.getDbContext();

            
            if (resetDb || !_productionDomainContext.CustomerOrders.Any())
            {
                _productionDomainContext.Database.EnsureDeleted();
                _productionDomainContext.Database.EnsureCreated();

                // using the same dataset as in krockert's master theses presentation 
                MasterDBInitializerSmall.DbInitialize(_productionDomainContext);
                LOGGER.Info("master data created.");
                OrderGenerator.GenerateOrdersSyncron(_productionDomainContext, ContextTest.TestConfiguration(), 1,
                    true); // .RunSynchronously();
                LOGGER.Info("Orders created.");
            }
        }

        private void InitModules()
        {
        }

        public void Start()
        {
            LOGGER.Info("Starting: ZPP simulation.");
            
            // reading orders from db
            List<T_CustomerOrder> CustomerOrders = _productionDomainContext.CustomerOrders.AsList();
            
            // TODO: here should be a MasterDBInitializerSmall (how to via akkaSim?)
            String sql = "Select * from dbo.T_CustomerOrder where CreationTime=(select min(CreationTime) from dbo.T_CustomerOrder)";
            _orderManager.Order(_productionDomainContext.CustomerOrders.FromSql(sql).AsList());
            
            LOGGER.Info("Finished: ZPP simulation.");
        }
    }
}