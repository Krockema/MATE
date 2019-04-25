using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Stock : BaseEntity
    {
        public const string ARTICLE_FKEY = "ArticleForeignKey";
        public string Name { get; set; }
        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public decimal StartValue { get; set; }
        public decimal Current { get; set; }
        public int ArticleForeignKey { get; set; }
        [JsonIgnore]
        public ICollection<T_StockExchange> StockExchanges { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        [JsonIgnore]
        public virtual ICollection<DemandStock> DemandStocks { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProviderStock> DemandProviderStocks { get; set; }
    }
}
