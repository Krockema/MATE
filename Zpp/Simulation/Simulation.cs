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
        private readonly DbCache _dbCache;

        private readonly StockManager _stockManager;
        

        private readonly PurchaseManager _purchaseManager;

        private readonly bool _resetDb = false;

        public Simulation()
        {
            LOGGER.Debug("Starting preparation for the ZPP simulation.");

            /*InitDb(_resetDb);

            _dbCache = new DbCache(_productionDomainContext);
            _purchaseManager = new PurchaseManager(_dbCache);
            ProductionManager _productionManager = new ProductionManager(_dbCache);
            _customerManager = new CustomerManager.CustomerManager(new DbCache(_productionDomainContext));
            _stockManager = new StockManager(_productionDomainContext, _productionManager, _purchaseManager);*/

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
                LOGGER.Debug("master data created.");
                OrderGenerator.GenerateOrdersSyncron(_productionDomainContext,
                    ContextTest.TestConfiguration(), 1,
                    true);
                LOGGER.Debug("Orders created.");
            }
        }
        

        public void Start()
        {
            LOGGER.Debug("Starting: ZPP simulation.");

            // TODO

            LOGGER.Debug("Finished: ZPP simulation.");
        }
    }
}