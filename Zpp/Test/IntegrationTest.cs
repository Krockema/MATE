using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DB;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Master40.SimulationCore.Helper;
using Master40.XUnitTest.DBContext;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Zpp.Test
{
    public class IntegrationTest : AbstractTest
    {
        public IntegrationTest() : base()
        {
            // TODO: orderQuantity should be set to higherValue (from simConfigs)
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, 1);
        }

        [Fact]
        public void TestMrpRun()
        {
            List<int> countsMasterDataBefore = countMasterData();
            IDbCache dbCache = new DbCache(ProductionDomainContext);
            Assert.True(dbCache.T_DemandsGetAll().Count == 1, "No demands are initially available.");
            
            MrpRun.RunMrp(dbCache, dbCache.DemandsGetAll());
            
            int expectedNumberOfDemandsAndProviders = 28;
            List<T_Demand> actualDemands = dbCache.T_DemandsGetAll();
            List<T_Provider> actualProviders = dbCache.T_ProvidersGetAll();
            Assert.True(actualDemands.Count == expectedNumberOfDemandsAndProviders + 1, // TODO: why is + 1 needed? 
                $"No demands were created by MrpRun: Expected {expectedNumberOfDemandsAndProviders}, " +
                $"Actual {actualProviders.Count}");
            Assert.True(actualProviders.Count == expectedNumberOfDemandsAndProviders, 
                $"No providers were created by MrpRun: Expected {expectedNumberOfDemandsAndProviders}, " +
                $"Actual {actualProviders.Count}");

            // check certain constraints are not violated
            
            // masterData must not change within an MrpRun
            List<int> countsMasterDataAfter = countMasterData();
            Assert.True(countsMasterDataBefore.SequenceEqual(countsMasterDataAfter),
                $"MasterData has changed, which should not be modified by MrpRun: " +
                $"\nBefore: {String.Join(", ", countsMasterDataBefore)}" + 
                $"\nAfter: {String.Join(", ", countsMasterDataAfter)}");
        }

        private List<int> countMasterData()
        {
            List<int> counts = new List<int>();
            counts.Add(ProductionDomainContext.Articles.Count());
            counts.Add(ProductionDomainContext.ArticleBoms.Count());
            counts.Add(ProductionDomainContext.ArticleTypes.Count());
            counts.Add(ProductionDomainContext.ArticleToBusinessPartners.Count());
            counts.Add(ProductionDomainContext.BusinessPartners.Count());
            counts.Add(ProductionDomainContext.Machines.Count());
            counts.Add(ProductionDomainContext.MachineGroups.Count());
            counts.Add(ProductionDomainContext.MachineTools.Count());
            counts.Add(ProductionDomainContext.Stocks.Count());
            counts.Add(ProductionDomainContext.Units.Count());
            counts.Add(ProductionDomainContext.Operations.Count());
            return counts;
        }
    }
}