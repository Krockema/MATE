using System.Linq;
using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.SimulationMrp.impl.Confirmation;
using Master40.SimulationMrp.impl.Confirmation.impl;
using Master40.SimulationMrp.impl.CustomerOrder;
using Master40.SimulationMrp.impl.CustomerOrder.impl;
using Zpp;
using Zpp.DataLayer;
using Zpp.Util;
using Zpp.Util.Performance;

namespace Master40.SimulationMrp.impl
{
    public class ZppSimulator : IZppSimulator
    {
       
        private readonly IConfirmationManager _confirmationManager = new ConfirmationManager();
        private ICustomerOrderCreator _customerOrderCreator = null;

        public ZppSimulator()
        {
        }

        public void StartOneCycle(SimulationInterval simulationInterval)
        {
            StartOneCycle(simulationInterval, null);
        }

        public void StartOneCycle(SimulationInterval simulationInterval,
            Quantity customerOrderQuantity)
        {
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            
            _customerOrderCreator.CreateCustomerOrders(simulationInterval, customerOrderQuantity);

            // Mrp2
            Zpp.Mrp2.impl.Mrp2 mrp2 = new Zpp.Mrp2.impl.Mrp2(simulationInterval);
            mrp2.StartMrp2();
            DebuggingTools.PrintStateToFiles(simulationInterval, dbTransactionData, "1_after_mrp2",
                true);

            // CreateConfirmations
            _confirmationManager.CreateConfirmations(simulationInterval);
            DebuggingTools.PrintStateToFiles(simulationInterval, dbTransactionData,
                "2_after_create_confirmations", false);

            // ApplyConfirmations
            // TODO: disable these two lines
            /*DemandToProviderGraph demandToProviderGraph = new DemandToProviderGraph();
            string demandToProviderGraphString = demandToProviderGraph.ToString();
            ZppConfiguration.CacheManager.UseArchiveForGetters();
            DemandToProviderGraph demandToProviderGraphArchive = new DemandToProviderGraph();
            string demandToProviderGraphArchiveString = demandToProviderGraphArchive.ToString();
            ZppConfiguration.CacheManager.UseArchiveForGettersRevert();*/
            
            _confirmationManager.ApplyConfirmations();

            DebuggingTools.PrintStateToFiles(simulationInterval, dbTransactionData,
                "3_after_apply_confirmations", false);
            
            // TODO: disable following lines
            /* DemandToProviderGraph demandToProviderGraph2 = new DemandToProviderGraph();
            string demandToProviderGraphString2 = demandToProviderGraph2.ToString();
            /*ZppConfiguration.CacheManager.UseArchiveForGetters();
            DemandToProviderGraph demandToProviderGraphArchive2 = new DemandToProviderGraph();
            string demandToProviderGraphArchiveString2 = demandToProviderGraphArchive2.ToString();
            ZppConfiguration.CacheManager.UseArchiveForGettersRevert();*/
        }

        /**
         * no confirmations are created and applied
         */
        public void StartTestCycle(bool shouldPersist=true)
        {
            int simulationInterval =
                ZppConfiguration.CacheManager.GetTestConfiguration().SimulationInterval;
            StartTestCycle(new SimulationInterval(0, simulationInterval), shouldPersist);
        }

        /**
         * no confirmations are created and applied
         */
        private void StartTestCycle(SimulationInterval simulationInterval, bool shouldPersist)
        {
            Quantity customerOrderQuantity = new Quantity(ZppConfiguration.CacheManager
                .GetTestConfiguration().CustomerOrderPartQuantity);


            // init transactionData
            ZppConfiguration.CacheManager.ReloadTransactionData();

            _customerOrderCreator = new CustomerOrderCreator(customerOrderQuantity);
            _customerOrderCreator.CreateCustomerOrders(simulationInterval, customerOrderQuantity);

            // execute mrp2
            Zpp.Mrp2.impl.Mrp2 mrp2 = new Zpp.Mrp2.impl.Mrp2(simulationInterval);
            mrp2.StartMrp2();

            DebuggingTools.PrintStateToFiles(simulationInterval,
                ZppConfiguration.CacheManager.GetDbTransactionData(), "", true);

            // no confirmations

            // persisting cached/created data
            if (shouldPersist)
            {
                ZppConfiguration.CacheManager.Persist();   
            }
        }

        public void StartPerformanceStudy(bool shouldPersist)
        {
            ZppConfiguration.IsInPerformanceMode = true;

            int maxSimulatingTime = ZppConfiguration.CacheManager.GetTestConfiguration()
                .SimulationMaximumDuration;
            int defaultInterval =
                ZppConfiguration.CacheManager.GetTestConfiguration().SimulationInterval;
            Quantity customerOrderQuantity = new Quantity(ZppConfiguration.CacheManager
                .GetTestConfiguration().CustomerOrderPartQuantity);

            _customerOrderCreator = new CustomerOrderCreator(customerOrderQuantity);

            string performanceLogCycles = "[";

            for (int i = 0; i * defaultInterval <= maxSimulatingTime; i++)
            {
                SimulationInterval simulationInterval =
                    new SimulationInterval(i * defaultInterval, defaultInterval - 1);

                StartOneCycle(simulationInterval, customerOrderQuantity);

                if (ZppConfiguration.CacheManager.GetDbTransactionDataArchive()
                        .CustomerOrderPartGetAll().Count() > customerOrderQuantity.GetValue())
                {
                    break;
                }
                
            }
            // TODO: here is no performance measure anymore, remove whole surounding method ?
            // DebuggingTools.PrintStateToFiles(dbTransactionData, true);
            DebuggingTools.WritePerformanceLog(performanceLogCycles);

            // persisting cached/created data
            if (shouldPersist)
            {
                ZppConfiguration.CacheManager.Persist();
            }
        }

        /**
         * without confirmations
         */
        public void StartMultipleTestCycles()
        {
            const int maxSimulatingTime = 5000;
            int defaultInterval =
                ZppConfiguration.CacheManager.GetTestConfiguration().SimulationInterval;

            // for (int i = 0; i * _interval < maxSimulatingTime; i++)
            for (int i = 0; i * defaultInterval < maxSimulatingTime; i++)
            {
                SimulationInterval simulationInterval =
                    new SimulationInterval(i * defaultInterval, defaultInterval - 1);
                StartTestCycle(simulationInterval, true);
            }
        }
    }
}