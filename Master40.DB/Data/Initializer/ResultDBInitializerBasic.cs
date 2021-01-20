using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;

namespace Master40.DB.Data.Initializer
{
    public static class ResultDBInitializerBasic
    {
        //[ThreadStatic] 
        private static int _simulationId = 1;
        public static void DbInitialize(ResultContext context)
        {
            context.Database.EnsureCreated();

            if (context.ConfigurationItems.Any())
            {
                return; // DB has been seeded
            }

            var configurationItems = new List<ConfigurationItem>()
            {
                new ConfigurationItem
                    {Property = "SimulationId", PropertyValue = _simulationId.ToString(), Description = "selfOrganizing with normal Queue"},
                new ConfigurationItem {Property = "SimulationNumber", PropertyValue = "1", Description = "Default"},
                new ConfigurationItem {Property = "SimulationKind", PropertyValue = "Default", Description = "Default"},
                new ConfigurationItem {Property = "OrderArrivalRate", PropertyValue = "0,025", Description = "Default"},
                new ConfigurationItem {Property = "OrderQuantity", PropertyValue = "1500", Description = "Default"},
                new ConfigurationItem {Property = "EstimatedThroughPut", PropertyValue = "1920", Description = "Default"},
                new ConfigurationItem {Property = "DebugAgents", PropertyValue = "false", Description = "Default"},
                new ConfigurationItem {Property = "DebugSystem", PropertyValue = "false", Description = "Default"},
                new ConfigurationItem {Property = "KpiTimeSpan", PropertyValue = "480", Description = "Default"},
                new ConfigurationItem {Property = "MinDeliveryTime", PropertyValue = "1440", Description = "Default"},
                new ConfigurationItem {Property = "MaxDeliveryTime", PropertyValue = "2400", Description = "Default"},
                new ConfigurationItem {Property = "TransitionFactor", PropertyValue = "3", Description = "Default"},
                new ConfigurationItem {Property = "MaxBucketSize", PropertyValue = "960", Description = "Default"},
                new ConfigurationItem {Property = "TimePeriodForThroughputCalculation", PropertyValue = "1920", Description = "Default"},
                new ConfigurationItem {Property = "Seed", PropertyValue = "1337", Description = "Default"},
                new ConfigurationItem {Property = "SettlingStart", PropertyValue = "2880", Description = "Default"},
                new ConfigurationItem {Property = "SimulationEnd", PropertyValue = "40320", Description = "Default"},
                new ConfigurationItem {Property = "WorkTimeDeviation", PropertyValue = "0.2", Description = "Default"},
                new ConfigurationItem {Property = "SaveToDB", PropertyValue = "true", Description = "Default"},
                new ConfigurationItem {Property = "TimeToAdvance", PropertyValue = "0", Description = "Default"},
                new ConfigurationItem {Property = "PriorityRule", PropertyValue = DB.Nominal.PriorityRule.LST.ToString(), Description = "Default, LST"}
            };
            context.ConfigurationItems.AddRange(entities: configurationItems);
            context.SaveChanges();
            AssertConfigurations(context, configurationItems, _simulationId);

            CreateSimulation(context, new List<ConfigurationItem> { new ConfigurationItem { Property  = "OrderArrivalRate", PropertyValue = "0,0275" }}, "Higher Arrival Rate");
            CreateSimulation(context, new List<ConfigurationItem> { new ConfigurationItem { Property  = "OrderArrivalRate", PropertyValue = "0,02" }}, "Lower Arrival Rate");
            CreateSimulation(context, new List<ConfigurationItem> { new ConfigurationItem { Property  = "OrderArrivalRate", PropertyValue = "0,01" }}, "Super low Arrival Rate");
            CreateSimulation(context, new List<ConfigurationItem> { new ConfigurationItem { Property  = "EstimatedThroughPut", PropertyValue = "1440" }}, "Estimated Throguh Put: 1440");

            _simulationId = 1;
        }

        private static void CreateSimulation(ResultContext context, List<ConfigurationItem> items, string description)
        {
            _simulationId++;
            var configurationItems = new List<ConfigurationItem>
            {
                new ConfigurationItem {Property = "SimulationId", PropertyValue = _simulationId.ToString(), Description = description },
            };
            items.ForEach(x => configurationItems.Add(x));

            context.ConfigurationItems.AddRange(configurationItems);
            context.SaveChanges();
            AssertConfigurations(context, configurationItems, _simulationId);
        }


        private static void AssertConfigurations(ResultContext context, List<ConfigurationItem> configurationItems, int simulationId)
        {
            var configurationRelations = new List<ConfigurationRelation>();
            var simId = int.Parse(configurationItems.Single(x => x.Property == "SimulationId").PropertyValue);
            foreach (var item in configurationItems)
            {
                if (item.Id != 1)
                {
                    configurationRelations.Add(new ConfigurationRelation {ParentItemId = simId, ChildItemId = item.Id, Id = simulationId });
                }
            }

            context.ConfigurationRelations.AddRange(entities: configurationRelations);
            context.SaveChanges();
        }

    }
}
