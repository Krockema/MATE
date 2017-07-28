using System.Collections.Generic;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class ProductionOrderBom : BaseEntity
    {
        public int ProductionOrderParentId { get; set; }
        [JsonIgnore]
        public ProductionOrder ProductionOrderParent { get; set; }
        /*public int? ProductionOrderChildId { get; set; }
        [JsonIgnore]
        public ProductionOrder ProductionOrderChild { get; set; }*/
        [JsonIgnore]
        public virtual ICollection<DemandProductionOrderBom> DemandProductionOrderBoms { get; set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
