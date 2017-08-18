using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Master40.DB;
using Master40.DB.Data.Helper;
using Master40.DB.Enums;
using Microsoft.EntityFrameworkCore;

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
        public static void CopyAllTables(this ProductionDomainContext sourceContext,
            ProductionDomainContext targetContext)
        {

            // basic Set
            targetContext.ArticleTypes.AddRange(sourceContext.ArticleTypes);
            targetContext.Units.AddRange(sourceContext.Units);
            targetContext.Machines.AddRange(sourceContext.Machines);
            targetContext.MachineGroups.AddRange(sourceContext.MachineGroups);
            targetContext.MachineTools.AddRange(sourceContext.MachineTools);
            targetContext.Articles.AddRange(sourceContext.Articles);
            targetContext.Stocks.AddRange(sourceContext.Stocks);
            targetContext.StockExchanges.AddRange(sourceContext.StockExchanges);
            targetContext.WorkSchedules.AddRange(sourceContext.WorkSchedules);
            targetContext.ArticleBoms.AddRange(sourceContext.ArticleBoms);
            targetContext.BusinessPartners.AddRange(sourceContext.BusinessPartners);
            targetContext.ArticleToBusinessPartners.AddRange(sourceContext.ArticleToBusinessPartners);
            targetContext.Orders.AddRange(sourceContext.Orders);
            targetContext.OrderParts.AddRange(sourceContext.OrderParts);
            targetContext.SimulationConfigurations.AddRange(sourceContext.SimulationConfigurations);
          

            targetContext.SaveChanges();
        }
        public static void LoadInMemoryDb(this ProductionDomainContext targetContext,
            SimulationDbState sourceContext)
        {
            targetContext.ArticleTypes.AddRange(sourceContext.ArticleTypes);
            targetContext.SaveChanges();
            targetContext.Units.AddRange(sourceContext.Units);
            targetContext.Machines.AddRange(sourceContext.Machines);
            targetContext.MachineGroups.AddRange(sourceContext.MachineGroups);
            targetContext.MachineTools.AddRange(sourceContext.MachineTools);
            targetContext.Articles.AddRange(sourceContext.Articles);
            targetContext.Stocks.AddRange(sourceContext.Stocks);
            targetContext.StockExchanges.AddRange(sourceContext.StockExchanges);
            targetContext.WorkSchedules.AddRange(sourceContext.WorkSchedules);
            targetContext.ArticleBoms.AddRange(sourceContext.ArticleBoms);
            targetContext.BusinessPartners.AddRange(sourceContext.BusinessPartners);
            targetContext.ArticleToBusinessPartners.AddRange(sourceContext.ArticleToBusinessPartners);
            targetContext.Orders.AddRange(sourceContext.Orders);
            targetContext.OrderParts.AddRange(sourceContext.OrderParts);

            // should be emty
            /*
            targetContext.ProductionOrders.AddRange(sourceContext.ProductionOrders);
            targetContext.ProductionOrderBoms.AddRange(sourceContext.ProductionOrderBoms);
            targetContext.ProductionOrderWorkSchedules.AddRange(sourceContext.ProductionOrderWorkSchedule);
            targetContext.Demands.AddRange(sourceContext.Demands);
            targetContext.Purchases.AddRange(sourceContext.Purchases);
            targetContext.PurchaseParts.AddRange(sourceContext.PurchaseParts);
            */
            
            targetContext.SaveChanges();
        }



        public static SimulationDbState SaveSimulationState(this ProductionDomainContext sourceContext)
        {
            var targetContext =
                new SimulationDbState
                {
                    ArticleTypes = sourceContext.ArticleTypes.ToList(),
                    Units = sourceContext.Units.ToList(),
                    Machines = sourceContext.Machines.ToList(),
                    MachineTools = sourceContext.MachineTools.ToList(),
                    MachineGroups = sourceContext.MachineGroups.ToList(),
                    Articles = sourceContext.Articles.ToList(),
                    Stocks = sourceContext.Stocks.ToList(),
                    StockExchanges = sourceContext.StockExchanges.ToList(),
                    WorkSchedules = sourceContext.WorkSchedules.ToList(),
                    ArticleBoms = sourceContext.ArticleBoms.ToList(),
                    BusinessPartners = sourceContext.BusinessPartners.ToList(),
                    Orders = sourceContext.Orders.ToList(),
                    OrderParts = sourceContext.OrderParts.ToList(),
                    Kpi = sourceContext.Kpis.ToList(),
                    ArticleToBusinessPartners = sourceContext.ArticleToBusinessPartners.ToList(),
                    ProductionOrders = sourceContext.ProductionOrders.ToList(),
                    ProductionOrderBoms = sourceContext.ProductionOrderBoms.ToList(),
                    ProductionOrderWorkSchedule = sourceContext.ProductionOrderWorkSchedules.ToList(),
                    Purchases = sourceContext.Purchases.ToList(),
                    PurchaseParts = sourceContext.PurchaseParts.ToList(),
                    Demands = sourceContext.Demands.ToList()
                };
            return targetContext;
            /*
var dbsets = _sourceContext.GetType().GetProperties().Where(x => x.DeclaringType == typeof(MasterDBContext)).ToList();
foreach (var dbset in dbsets)
{
    dynamic propValue = dbset.GetValue(_sourceContext);
    Type type =  Type.GetType("Master40.DB.DB.Models.Article");

    var listType = typeof(List<>);
    var constructedListType = listType.MakeGenericType(Type.GetType(dbset.Name));
    var instance = Activator.CreateInstance(constructedListType);

    var tolist = typeof(Enumerable).GetMethod("ToList");


    tolist = tolist.MakeGenericMethod(type);
    var ret = tolist.Invoke(type, propValue);

}
*/
            // Units

            //_targetContext.SaveChanges();
        }


        /// <summary>
        /// Sadly doesnt work yet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anyTable"></param>
        /// <param name="targetContext"></param>
        private static void LoadTable<T>(this T anyTable, CopyContext targetContext)
        {
            // Start Transaction because Identity insert "ON" is only for ONE transaction valid
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                // get get Tablename
                var name = anyTable.GetType().GenericTypeArguments[0].Name + "s";
                // set Table Identity Insert ON
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT " + name + " ON;");
                // for each item in Table
                foreach (var item in (IEnumerable)anyTable)
                {
                    // Copy Propertys to new item -> Has to be outside of the add Command otherwhise it fails
                    var chunk = item.CopyProperties();
                    // add New Item
                    targetContext.Add(chunk);
                }
                // Save Changes, Revert Identity Insert, and commit Transaction
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT " + name + " OFF");
                transaction.Commit();
            }
        }


        public static void LoadContextFromSimulation(this CopyContext targetContext, SimulationDbState sourceContext)
        {


            sourceContext.ArticleTypes.LoadTable(targetContext);
            //sourceContext.ArticleTypes.LoadTable(targetContext);
            // SimulationDbState simulationDbState = Newtonsoft.Json.JsonConvert.DeserializeObject<SimulationDbState>(simulationContext);
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ArticleTypes ON;");
                targetContext.ArticleTypes.AddRange(sourceContext.ArticleTypes);
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ArticleTypes OFF");
                transaction.Commit();
            }
            // units
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Units ON;");
                foreach (var item in sourceContext.Units)
                {
                    targetContext.Add(item);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Units OFF");
                transaction.Commit();
            }

            // MachineGroups
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT MachineGroups ON;");
                foreach (var item in sourceContext.MachineGroups)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT MachineGroups OFF");
                transaction.Commit();
            }

            // Machines
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Machines ON;");
                foreach (var item in sourceContext.Machines)
                {
                    targetContext.Add(item);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Machines OFF");
                transaction.Commit();
            }


            // MachineTools
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT MachineTools ON;");
                foreach (var item in sourceContext.MachineTools)
                {
                    targetContext.Add(item);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT MachineTools OFF");
                transaction.Commit();
            }

            // Articles
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Articles ON;");
                foreach (var item in sourceContext.Articles)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Articles OFF");
                transaction.Commit();
            }

            // Stocks
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Stocks ON;");
                targetContext.Stocks.AddRange(sourceContext.Stocks);
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Stocks OFF");
                transaction.Commit();
            }

            // StockExchanges
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT StockExchanges ON;");
                targetContext.StockExchanges.AddRange(sourceContext.StockExchanges);
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT StockExchanges OFF");
                transaction.Commit();
            }
            // WorkSchedules
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT WorkSchedule ON;");
                foreach (var item in sourceContext.WorkSchedules)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT WorkSchedule OFF");
                transaction.Commit();
            }
            // ArticleBoms
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ArticleBoms ON;");
                foreach (var item in sourceContext.ArticleBoms)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ArticleBoms OFF");
                transaction.Commit();
            }

            // BusinessPartners
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT BusinessPartners ON;");
                foreach (var item in sourceContext.BusinessPartners)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT BusinessPartners OFF");
                transaction.Commit();
            }

            // ArticleToBusinessPartners
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ArticleToBusinessPartners ON;");
                foreach (var item in sourceContext.ArticleToBusinessPartners)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ArticleToBusinessPartners OFF");
                transaction.Commit();
            }

            // Orders
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Orders ON;");
                foreach (var item in sourceContext.Orders)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Orders OFF");
                transaction.Commit();
            }

            // ArticleBoms
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT OrderParts ON;");
                foreach (var item in sourceContext.OrderParts)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT OrderParts OFF");
                transaction.Commit();
            }
            // ProductionOrders
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ProductionOrders ON;");
                foreach (var item in sourceContext.ProductionOrders)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ProductionOrders OFF");
                transaction.Commit();
            }

            // ProductionOrderBoms
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ProductionOrderBoms ON;");
                foreach (var item in sourceContext.ProductionOrderBoms)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ProductionOrderBoms OFF");
                transaction.Commit();
            }

            // ProductionOrderWorkSchedule
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ProductionOrderWorkSchedule ON;");
                foreach (var item in sourceContext.ProductionOrderWorkSchedule)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT ProductionOrderWorkSchedule OFF");
                transaction.Commit();
            }

            // Purchases
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Purchases ON;");
                foreach (var item in sourceContext.Purchases)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Purchases OFF");
                transaction.Commit();
            }

            // PurchaseParts
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT PurchaseParts ON;");
                targetContext.PurchaseParts.AddRange(sourceContext.PurchaseParts);
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT PurchaseParts OFF");
                transaction.Commit();
            }
            // Demands
            using (var transaction = targetContext.Database.BeginTransaction())
            {
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Demands ON;");
                foreach (var item in sourceContext.Demands)
                {
                    var chunk = item.CopyProperties();
                    targetContext.Add(chunk);
                }
                targetContext.SaveChanges();
                targetContext.Database.ExecuteSqlCommand("SET IDENTITY_INSERT Demands OFF");
                transaction.Commit();
            }
        }
        

    }
}