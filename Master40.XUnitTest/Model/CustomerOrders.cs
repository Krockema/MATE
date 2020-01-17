using Master40.DB;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper;
using Master40.DB.Data.Initializer;
using Master40.DB.Nominal;
using Master40.SimulationCore.Environment;
using Master40.SimulationCore.Environment.Options;
using Xunit;
using DBConnectionString = Master40.SimulationCore.Environment.Options.DBConnectionString;

using SimulationId = Master40.SimulationCore.Environment.Options.SimulationId;
using SimulationNumber = Master40.SimulationCore.Environment.Options.SimulationNumber;

namespace Master40.XUnitTest.Model
{
    public class CustomerOrderCheck
    {
        /*
        private DataBase<ProductionDomainContext> DataBase;
        private Configuration config;
        public CustomerOrderCheck()
        {
            DataBase = Dbms.GetNewMasterDataBase();
            MasterDBInitializerTruck.DbInitialize(context: DataBase.DbContext);
        }

        [Fact]
        public void CustomerOrders()
        {
            config = SimulationCore.Environment.Configuration.Create(args: new object[]
                                                {
                                                    // set ResultDBString and set SaveToDB true
                                                    , new SimulationId(value: 1)
                                                    , new SimulationNumber(value: 1)
                                                    , new OrderArrivalRate(value: 0.025)
                                                    , new OrderQuantity(value: 500)
                                                    , new Seed(value: 1337)
                                                    , new MinDeliveryTime(value: 1160)
                                                    , new MaxDeliveryTime(value: 1600)
                                                    , new SimulationEnd(value: 20160)
                                                    
                                                });
            OrderGenerator orderGenerator = new OrderGenerator(, );

            for (int i = 0; i < 500; i++)
            {
                
            }


            Assert.True(articles.All(x => x.ArticleBoms.Count >= 0));
        }
        */
    }
}
