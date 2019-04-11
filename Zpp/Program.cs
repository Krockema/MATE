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
using System.Runtime.InteropServices;

namespace Zpp
{
    class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static bool isWindows = System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(OSPlatform.Windows);

        static void Main(string[] args)
        {
            logger.Info("Starting ZPP.");

            testMethod();
        }

        private static void testMethod()
        {

            // If better sim performance is needed: InMemory
            // ProductionDomainContext _ctx = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
            // .UseInMemoryDatabase(databaseName: "InMemoryDB")
            // .Options); // InMemoryDB

            ProductionDomainContext _productionDomainContext;
            if (isWindows)
            {
                // Windows
                _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
               .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Zpp;Trusted_Connection=True;MultipleActiveResultSets=true")
               .Options);
            }
            else
            {
                // With Sql Server for Mac/Linux
                _productionDomainContext = new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                   .UseSqlServer("Server=localhost,1433;Database=Zpp;MultipleActiveResultSets=true;User ID=SA;Password=123*Start#")
                   .Options);
            }
            _productionDomainContext.Database.EnsureDeleted();
            _productionDomainContext.Database.EnsureCreated();
            MasterDBInitializerSmall.DbInitialize(_productionDomainContext);
            OrderGenerator.GenerateOrdersSyncron(_productionDomainContext, ContextTest.TestConfiguration(), 1, true); // .RunSynchronously();
            logger.Info("Orders created.");
        }

    }
}