using System.Collections.Generic;
using Master40.DB.Enums;
using Newtonsoft.Json;

namespace Master40.DB.DataModel
{
    public class T_ProductionOrderBom : BaseEntity
    {
        public static string PRODUCTIONORDERPARRENT_FKEY = "ProductionOrderParentId";
        public int ProductionOrderParentId { get; set; }
        [JsonIgnore]
        public T_ProductionOrder ProductionOrderParent { get; set; }
        [JsonIgnore]
        public virtual ICollection<DemandProductionOrderBom> DemandProductionOrderBoms { get; set; }
        public decimal Quantity { get; set; }
        public State State { get; set; }
    }
}
