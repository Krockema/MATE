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
using Zpp.DemandToProviderDomain;

namespace Zpp.Test
{
    public class IntegrationTest : AbstractTest
    {
        private const int _orderQuantity = 6;
        
        public IntegrationTest()
        {
            // TODO: orderQuantity should be set to higherValue (from simConfigs)
            OrderGenerator.GenerateOrdersSyncron(ProductionDomainContext,
                ContextTest.TestConfiguration(), 1, true, _orderQuantity);
        }

        [Fact]
        public void TestMrpRun()
        {
            List<int> countsMasterDataBefore = CountMasterData();
            
            Assert.True(ProductionDomainContext.CustomerOrders.Count() == _orderQuantity, "No customerOrders are initially available.");

            IPlan plan = MrpRun.RunMrp(ProductionDomainContext);
            
            IDemands actualDemands = plan.GetDemands();

            IDemandToProviders demandToProviders = plan.GetDemandToProviders().ToDemandToProviders(plan.GetDbTransactionData());
            foreach (var demand in actualDemands.GetAll())
            {
                bool isSatisfied = 
                    demandToProviders.IsSatisfied(demand);
                Assert.True(isSatisfied, $"The demand {demand} should be satisfied, but it is NOT.");
            }

            // check certain constraints are not violated
            
            // masterData must not change within an MrpRun
            List<int> countsMasterDataAfter = CountMasterData();
            Assert.True(countsMasterDataBefore.SequenceEqual(countsMasterDataAfter),
                $"MasterData has changed, which should not be modified by MrpRun: " +
                $"\nBefore: {String.Join(", ", countsMasterDataBefore)}" + 
                $"\nAfter: {String.Join(", ", countsMasterDataAfter)}");
        }
        

        private List<int> CountMasterData()
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
            counts.Add(ProductionDomainContext.CustomerOrders.Count());
            counts.Add(ProductionDomainContext.CustomerOrderParts.Count());
            return counts;
        }
    }
}