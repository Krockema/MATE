using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.DB;
using Master40.DB.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq.Clauses;

namespace Master40.Simulation.Simulation
{
    public static class CopyTableScope
    {
        /*
        public void SqlBulkCopy()
        {
            // Establishing connection
            SqlConnectionStringBuilder cb = new SqlConnectionStringBuilder();
            cb.DataSource =
                "Server=(localdb)\\mssqllocaldb;Database=Master40;Trusted_Connection=True;MultipleActiveResultSets=true";
            cb.InitialCatalog = "Master40";
            cb.IntegratedSecurity = true;
            SqlConnection cnn = new SqlConnection(cb.ConnectionString);

            // Getting source data
            SqlCommand cmd = new SqlCommand("SELECT * FROM PendingOrders", cnn);
            cnn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();

            // Initializing an SqlBulkCopy object
            SqlBulkCopy sbc = new SqlBulkCopy("server=.;database=ProductionTest;Integrated Security=SSPI");

            // Copying data to destination
            sbc.DestinationTableName = "Temp";
            sbc.WriteToServer(rdr);

            // Closing connection and the others
            sbc.Close();
            cnn.Close();
        }
        */
        public static void CopyAllTables(this ProductionDomainContext _sourceContext,
            ProductionDomainContext _targetContext)
        {

            // basic Set
            _targetContext.ArticleTypes.AddRange(_sourceContext.ArticleTypes);

            _targetContext.Units.AddRange(_sourceContext.Units);
            _targetContext.Machines.AddRange(_sourceContext.Machines);
            _targetContext.MachineTools.AddRange(_sourceContext.MachineTools);
            _targetContext.Articles.AddRange(_sourceContext.Articles);
            _targetContext.Stocks.AddRange(_sourceContext.Stocks);
            _targetContext.WorkSchedules.AddRange(_sourceContext.WorkSchedules);
            _targetContext.ArticleBoms.AddRange(_sourceContext.ArticleBoms);
            _targetContext.BusinessPartners.AddRange(_sourceContext.BusinessPartners);
            _targetContext.Orders.AddRange(_sourceContext.Orders);
            _targetContext.OrderParts.AddRange(_sourceContext.OrderParts);


            _targetContext.ProductionOrders.AddRange(_sourceContext.ProductionOrders);
            _targetContext.ProductionOrderBoms.AddRange(_sourceContext.ProductionOrderBoms);
            _targetContext.ProductionOrderWorkSchedule.AddRange(_sourceContext.ProductionOrderWorkSchedule);
            _targetContext.Purchases.AddRange(_sourceContext.Purchases);
            _targetContext.PurchaseParts.AddRange(_sourceContext.PurchaseParts);
            _targetContext.Demands.AddRange(_sourceContext.Demands);

            _targetContext.SaveChanges();
        }

        public static void SetSimulationProperties(this DbSet<BaseEntity> table, string simId, SimulationType simType)
        {
            foreach (var item in table)
            {
                item.SetSimulationItemProperties(simType, simId);
            }
        }

        public static void SetSimulationItemProperties(this BaseEntity item, SimulationType simType, string simId)
        {
            item.SimulationType = simType;
            item.SimulationIdent = simId;
        }

        public static T SaveDbPropertiesTo<T>(this T source, string simId, SimulationType simType)
        {
            var plist = from prop in typeof(T).GetProperties() where prop.CanRead && prop.CanWrite select prop;
            var item = Activator.CreateInstance<T>();
            foreach (PropertyInfo prop in plist)
            {
                if (prop.Name == "SimulationIdent")
                    prop.SetValue(item, simId, null);
                else if (prop.Name == "SimulationType")
                    prop.SetValue(item, simType, null);
                else if (prop.Name != "Id")
                    prop.SetValue(item, prop.GetValue(source, null), null);
            }
            return item;
        }

        public static void SaveSimulationState(this ProductionDomainContext _sourceContext
            , ProductionDomainContext _targetContext
            , string simId
            , SimulationType simType)
        {
            // ArticlesTypes
            foreach (var item in _sourceContext.ArticleTypes)
            {
                _targetContext.Add(item.SaveDbPropertiesTo(simId, simType));
            }

            // Units
            foreach (var item in _sourceContext.Units)
            {
                _targetContext.Add(item.SaveDbPropertiesTo(simId, simType));
            }

            // Machines
            foreach (var item in _sourceContext.Machines)
            {
                _targetContext.Add(item.SaveDbPropertiesTo(simId, simType));
            }
            // MachineTools
            foreach (var item in _sourceContext.Machines)
            {
                _targetContext.Add(item.SaveDbPropertiesTo(simId, simType));
            }

            _targetContext.MachineTools.AddRange(_sourceContext.MachineTools);
            _targetContext.Articles.AddRange(_sourceContext.Articles);
            _targetContext.Stocks.AddRange(_sourceContext.Stocks);
            _targetContext.WorkSchedules.AddRange(_sourceContext.WorkSchedules);
            _targetContext.ArticleBoms.AddRange(_sourceContext.ArticleBoms);
            _targetContext.BusinessPartners.AddRange(_sourceContext.BusinessPartners);
            _targetContext.Orders.AddRange(_sourceContext.Orders);
            _targetContext.OrderParts.AddRange(_sourceContext.OrderParts);


            _targetContext.ProductionOrders.AddRange(_sourceContext.ProductionOrders);
            _targetContext.ProductionOrderBoms.AddRange(_sourceContext.ProductionOrderBoms);
            _targetContext.ProductionOrderWorkSchedule.AddRange(_sourceContext.ProductionOrderWorkSchedule);
            _targetContext.Purchases.AddRange(_sourceContext.Purchases);
            _targetContext.PurchaseParts.AddRange(_sourceContext.PurchaseParts);
            _targetContext.Demands.AddRange(_sourceContext.Demands);




            _targetContext.SaveChanges();
        }
    }
}