using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.StockManager
{
    public class StockManager
    {
        private readonly Queue<T_StockExchange> _stockExchanges = new Queue<T_StockExchange>();
        
        public bool HasEnough(M_Article article)
        {
            return true;
        }

        public void Order(M_Article article)
        {
            _stockExchanges.Enqueue(new T_StockExchange());
        }
    }
}