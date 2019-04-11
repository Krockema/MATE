using System;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.Data.Initializer;
using Master40.DB.Models;
using Master40.Tools.Simulation;
using Master40.XUnitTest.DBContext;
using MathNet.Numerics.Distributions;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Zpp
{
    class Program
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        
        static void Main(string[] args)
        {
            
            logger.Info("Starting ZPP.");

                testMethod();
        }

        private static void testMethod()
        {
            
            // ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                // .UseInMemoryDatabase(databaseName: "InMemoryDB")
                // .Options); // InMemoryDB
                ProductionDomainContext _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Zpp;Trusted_Connection=True;MultipleActiveResultSets=true")
                    .Options);
                _productionDomainContext.Database.EnsureDeleted();
            _productionDomainContext.Database.EnsureCreated();
            MasterDBInitializerSmall.DbInitialize(_productionDomainContext);
            OrderGenerator.GenerateOrdersSyncron(_productionDomainContext, ContextTest.TestConfiguration(), 1, true); // .RunSynchronously();
            logger.Info("Orders created.");
        }
        
    }
}