using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Models;
using Master40.Tools.Simulation;
using Master40.XUnitTest.DBContext;
using MathNet.Numerics.Distributions;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Runtime.InteropServices;
using Dapper;


namespace Zpp.Simulation
{
    public class Simulation
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static readonly bool IS_WINDOWS = System.Runtime.InteropServices.RuntimeInformation
            .IsOSPlatform(OSPlatform.Windows);
        private ProductionDomainContext _productionDomainContext;
        
        
        // MRP modules
        private readonly OrderManager.OrderManager  _orderManager = new OrderManager.OrderManager();
        private readonly StockManager.StockManager  _stockManager = new StockManager.StockManager();
        private readonly ProductionManager.ProductionManager  _productionManager = new ProductionManager.ProductionManager();
        private readonly PurchaseManager.PurchaseManager  _purchaseManager = new PurchaseManager.PurchaseManager();

        public Simulation()
        {
            LOGGER.Info("Starting preparation for the ZPP simulation.");

            InitDb();
            InitModules();
        }

        private void InitDb()
        {
            // If better sim performance is needed: InMemory
            // ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            // .UseInMemoryDatabase(databaseName: "InMemoryDB")
            // .Options); // InMemoryDB

            
            if (IS_WINDOWS)
            {
                // Windows
                _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer(
                        "Server=(localdb)\\mssqllocaldb;Database=Zpp;Trusted_Connection=True;MultipleActiveResultSets=true")
                    .Options);
            }
            else
            {
                // With Sql Server for Mac/Linux
                _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer(
                        "Server=localhost,1433;Database=Zpp;MultipleActiveResultSets=true;User ID=SA;Password=123*Start#")
                    .Options);
            }

            _productionDomainContext.Database.EnsureCreated();
            if (!_productionDomainContext.Orders.Any())
            {
                _productionDomainContext.Database.EnsureDeleted();

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
            List<Order> orders = _productionDomainContext.Orders.AsList();
            
            // TODO: here should be a MasterDBInitializerSmall (how to via akkaSim?)
            String sql = "Select * from dbo.Orders where CreationTime=(select min(CreationTime) from dbo.Orders)";
            _orderManager.order(_productionDomainContext.Orders.FromSql(sql).AsList().ElementAt(0));
            
            LOGGER.Info("Finished: ZPP simulation.");
        }
    }
}