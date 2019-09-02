using System.Collections.Generic;
using Master40.DB.DataModel;
using Newtonsoft.Json;

namespace Zpp.MrpRun.StockManagement
{
    public class StockState
    {
        private string _stocksAsString;

        public void BackupStockState(List<M_Stock> stocks)
        {
            _stocksAsString = JsonConvert.SerializeObject(stocks);
        }
        
        public List<M_Stock> ResetStockState()
        {
            
            return JsonConvert.DeserializeObject<List<M_Stock>>(_stocksAsString);
        }
    }
}