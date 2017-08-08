using System.Collections.Generic;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.Models
{
    public class ProductionOrderBom : BaseEntity
    {
        public int ProductionOrderParentId { get; set; }
        [JsonIgnore]
        public ProductionOrder ProductionOrderParent { get; set; }
        [JsonIgnore]
        public virtual ICollection<DemandProductionOrderBom> DemandProductionOrderBoms { get; set; }
        public decimal Quantity { get; set; }
        public State State { get; set; }
    }
}
