using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Master40.DB.Models
{
    public class Stock : BaseEntity
    {
        public string Name { get; set; }
        public decimal Max { get; set; }
        public decimal Min { get; set; }
        public decimal Current { get; set; }

        public int ArticleForeignKey { get; set; }
        public Article Article { get; set; }
        public virtual ICollection<DemandStock> DemandStocks { get; set; }
        public virtual ICollection<DemandProviderStock> DemandProviderStocks { get; set; }
    }
}
