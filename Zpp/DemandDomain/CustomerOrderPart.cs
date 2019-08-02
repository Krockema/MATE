using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.WrappersForPrimitives;


namespace Zpp.DemandDomain
{
    public class CustomerOrderPart : Demand 
    {

        public CustomerOrderPart(IDemand demand, IDbMasterDataCache dbMasterDataCache) : base(demand, dbMasterDataCache)
        {
            
        }

        public override IDemand ToIDemand()
        {
            return (T_CustomerOrderPart)_demand;
        }

        public override M_Article GetArticle()
        {
            Id articleId = new Id(((T_CustomerOrderPart) _demand).ArticleId);
            return _dbMasterDataCache.M_ArticleGetById(articleId);
        }

        public override DueTime GetDueTime( )
        {
            T_CustomerOrderPart customerOrderPart = ((T_CustomerOrderPart) _demand);
            if (customerOrderPart.CustomerOrder != null)
            {
                return new DueTime(customerOrderPart.CustomerOrder.DueTime);
            }
            Id customerOrderId = new Id(customerOrderPart.CustomerOrderId);
            DueTime dueTime = new DueTime(_dbMasterDataCache.T_CustomerOrderGetById(customerOrderId).DueTime);
            return dueTime;
        }

        public override string GetGraphizString()
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"D(COP);{GetQuantity()};{GetArticle().Name}";
            return graphizString;
        }
    }
}