using Priority_Queue;
using Zpp.DataLayer;
using Zpp.DataLayer.impl.DemandDomain;
using Zpp.DataLayer.impl.DemandDomain.WrappersForCollections;
using Zpp.DataLayer.impl.WrapperForEntities;
using Zpp.Util;
using Zpp.Util.Queue;

namespace Zpp.Mrp2.impl.Mrp1.impl
{
    public class Mrp1 : IMrp1
    {
        private readonly Demands dbDemands;

        public Mrp1(Demands dbDemands)
        {
            this.dbDemands = dbDemands;
        }

        public void StartMrp1()
        {
            // init
            int MAX_DEMANDS_IN_QUEUE = 100000;

            FastPriorityQueue<DemandQueueNode> demandQueue =
                new FastPriorityQueue<DemandQueueNode>(MAX_DEMANDS_IN_QUEUE);

            IProviderManager providerManager = new ProviderManager();

            foreach (var demand in dbDemands)
            {
                demandQueue.Enqueue(new DemandQueueNode(demand), demand.GetStartTimeBackward().GetValue());
            }
            
            EntityCollector allCreatedEntities = new EntityCollector();
            while (demandQueue.Count != 0)
            {
                DemandQueueNode firstDemandInQueue = demandQueue.Dequeue();

                EntityCollector response =
                    MaterialRequirementsPlanningForOneDemand(firstDemandInQueue.GetDemand(), providerManager);
                allCreatedEntities.AddAll(response);
                
                foreach (var demand in response.GetDemands())
                {
                    demandQueue.Enqueue(new DemandQueueNode(demand),
                        demand.GetStartTimeBackward().GetValue());
                }
            }

            // write data to _dbTransactionData
            IDbTransactionData dbTransactionData =
                ZppConfiguration.CacheManager.GetDbTransactionData();
            dbTransactionData.AddAllFrom(allCreatedEntities);
            // End of MaterialRequirementsPlanning
        }
        
        private EntityCollector MaterialRequirementsPlanningForOneDemand(Demand demand,
            IProviderManager providerManager)
        {
            EntityCollector entityCollector = new EntityCollector();

            EntityCollector response = providerManager.Satisfy(demand, demand.GetQuantity());
            entityCollector.AddAll(response);
            response = providerManager.CreateDependingDemands(entityCollector.GetProviders());
            entityCollector.AddAll(response);

            if (entityCollector.IsSatisfied(demand) == false)
            {
                throw new MrpRunException($"'{demand}' was NOT satisfied: remaining is " +
                                          $"{entityCollector.GetRemainingQuantity(demand)}");
            }

            return entityCollector;
        }
    }
}