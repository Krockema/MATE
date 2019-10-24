using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class M_Stock : BaseEntity
    {
        public string Name { get; set; }
        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public decimal StartValue { get; set; }
        // Try to not use this, since it's hard to maintain: ZPP does not support this property
        public decimal Current { get; set; }
        public int ArticleForeignKey { get; set; }
        [JsonIgnore]
        public ICollection<T_StockExchange> StockExchanges { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Current}";
        }
    }
}
