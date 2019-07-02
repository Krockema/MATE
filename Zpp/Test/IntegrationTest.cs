using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Master40.DB;
using Zpp.DemandDomain;
using Zpp.ProviderDomain;
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
            
            Assert.True(ProductionDomainContext.CustomerOrders.Count() == 1, "No customerOrders are initially available.");
            
            IPlan plan = MrpRun.RunMrp(ProductionDomainContext);
            
            IDemands actualDemands = plan.GetDemands();

            foreach (var demand in actualDemands.GetAll())
            {
                bool isSatisfied = !plan.GetDemandToProviders().ToDemandToProviders()
                    .IsSatisfied(demand);
                Assert.True(isSatisfied, "The demand should be satisfied, but it is NOT.");
            }

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