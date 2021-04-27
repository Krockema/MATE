using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Interfaces;
using Mate.DataCore.Nominal;
using Newtonsoft.Json;

namespace Mate.DataCore.DataModel
{
    public class T_PurchaseOrderPart : BaseEntity, IProvider
    {
        public int PurchaseOrderId { get; set; }
        [JsonIgnore]
        public T_PurchaseOrder PurchaseOrder { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public State State { get; set; }

        
        
        public M_Article GetArticle()
        {
            return Article;
        }
        
        public Quantity GetQuantity()
        {
            return new Quantity(Quantity);
        }

    }
}
