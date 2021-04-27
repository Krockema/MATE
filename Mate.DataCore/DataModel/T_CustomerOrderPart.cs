using Mate.DataCore.Data.WrappersForPrimitives;
using Mate.DataCore.Interfaces;
using Mate.DataCore.Nominal;
using Newtonsoft.Json;

namespace Mate.DataCore.DataModel
{
    public class T_CustomerOrderPart : BaseEntity, IDemand
    {
        public int CustomerOrderId { get; set; }
        [JsonIgnore]
        public T_CustomerOrder CustomerOrder { get; set; }
        public int ArticleId { get; set; }
        [JsonIgnore]
        public M_Article Article { get; set; }
        public int Quantity { get; set; }
        [JsonIgnore]
        public bool IsPlanned { get; set; }
        public State State { get; set; }
        
        /*
        [NotMapped]
        public int RequesterId { get => this.OrderPartId; }
        public DemandToProvider DemandToDemand { get; set; }
        [NotMapped]
        public string Source { get; private set; }
        */

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


