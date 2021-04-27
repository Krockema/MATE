using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Interfaces;
using Newtonsoft.Json;

namespace Mate.DataCore.DataModel
{
    public class T_ProductionOrderBom : BaseEntity, IDemand
    {
        public int ProductionOrderParentId { get; set; }
        [JsonIgnore]
        public T_ProductionOrder ProductionOrderParent { get; set; }
        [JsonIgnore]
        public decimal Quantity { get; set; }
        public int? ProductionOrderOperationId { get; set; }
        public T_ProductionOrderOperation ProductionOrderOperation { get; set; }
        public int ArticleChildId { get; set; }
        [JsonIgnore]
        public M_Article ArticleChild { get; set; }
        
        public Quantity GetQuantity()
        {
            return new Quantity(Quantity);
        }

        public override string ToString()
        {
            return $"{ArticleChild.Name}: {ProductionOrderOperation}";
        }
    }
}
