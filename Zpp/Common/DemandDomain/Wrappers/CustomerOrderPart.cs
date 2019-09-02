using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.DataModel;
using Master40.DB.Interfaces;
using Zpp.DbCache;
using Zpp.WrappersForPrimitives;

namespace Zpp.Common.DemandDomain.Wrappers
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

        public override DueTime GetDueTime(IDbTransactionData dbTransactionData)
        {
            T_CustomerOrderPart customerOrderPart = ((T_CustomerOrderPart) _demand);
            if (customerOrderPart.CustomerOrder != null)
            {
                return new DueTime(customerOrderPart.CustomerOrder.DueTime);
            }
            Id customerOrderId = new Id(customerOrderPart.CustomerOrderId);
            customerOrderPart.CustomerOrder =
                _dbMasterDataCache.T_CustomerOrderGetById(customerOrderId);
            DueTime dueTime = new DueTime(customerOrderPart.CustomerOrder.DueTime);
            return dueTime;
        }

        public override string GetGraphizString(IDbTransactionData dbTransactionData)
        {
            // Demand(CustomerOrder);20;Truck
            string graphizString = $"D(COP);{base.GetGraphizString(dbTransactionData)}";
            return graphizString;
        }

        public override DueTime GetStartTime(IDbTransactionData dbTransactionData)
        {
            return null;
        }
    }
}